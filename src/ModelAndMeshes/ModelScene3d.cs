using System;
using System.Linq;
using Gizmo3DPlugin;
using Godot;

public partial class ModelScene3d : Node3D
{ //public Node3D MainMesh;

    [Export] public Gizmo3D Gizmo { get; private set; }
    bool hasNodeSelected => Gizmo.Selections.Count > 0;
    // [Export] public float RotationSpeed = 0.01f;
    // [Export] public float VerticalAngleLimit = 180.0f;  // Limit vertical angle to prevent flipping.
    // [Export] public float scrollSpeed = 1f;

    // private float _horizontalAngle = 0.0f;
    // private float _verticalAngle = 0.0f;
    // private const float Rad2Deg = 180f / Mathf.Pi;  // Define Rad2Deg manually.


    public override void _Ready()
    {
        //       MainMesh = GetChild<Node3D>(0);

        GlobalEvents.Instance.OnSpriteGeneratorMouseClick += OnSpriteGeneratorMouseClick;
        Gizmo.TransformChanged += OnGizmoTargetTransformChanged;
        AddChildrenStaticBodyCollisionShape();
    }

    private void OnGizmoTargetTransformChanged(int mode, Vector3 value)
    {
        //Mode 1 = Rotation // Mode 2 = Position // Mode 3 = Scale
        GD.PrintT($"GismoTransform ended value: {value} and mode: {mode}");
        //TODO: Update the Node ModelManager to sync the ModelPosition and ModelRotation
    }


    private void OnSpriteGeneratorMouseClick(InputEventMouseButton mouseButton)
    { //
        HandleGizmoSelection(Gizmo, mouseButton);
    }

    private void AddChildrenStaticBodyCollisionShape()
    {
        //find the pivot node (Model3DMainPivotControl)
        var pivotNode = GlobalUtil.GetAllChildNodesByType<Model3DMainPivotControl>(this).FirstOrDefault();
        //var pivotNode = GetChildren().FirstOrDefault(c => c.GetType() == typeof(Model3DMainPivotControl));
        // var pivotNode = GetNodeOrNull<Model3DMainPivotControl>("%Model3DPivotControl");

        var allChildMesheInstance3D = GlobalUtil.GetAllChildNodesByType<MeshInstance3D>(pivotNode);
        foreach (var meshInstance3dObj in allChildMesheInstance3D)
        {
            var allCollisionShapes = GlobalUtil.GetAllChildNodesByType<CollisionShape3D>(meshInstance3dObj);
            if (allCollisionShapes.Count == 0)
            {
                meshInstance3dObj.CreateConvexCollision();
                GD.PrintT($"Adding ConvexCollisionto: {meshInstance3dObj.Name}");
            }

        }
    }


    public override void _UnhandledInput(InputEvent @event)
    {

        // Enable/Disable Toggle modes
        // if (@event.IsActionPressed(moveMode))
        // 	Gizmo.Mode ^= Gizmo3D.ToolMode.Move;
        // if (@event.IsActionPressed(scaleMode))
        // 	Gizmo.Mode ^= Gizmo3D.ToolMode.Scale;
        // if (@event.IsActionPressed(rotateMode))
        // 	Gizmo.Mode ^= Gizmo3D.ToolMode.Rotate;

        // // Toggle between local and global space
        // if (!Gizmo.Editing && @event.IsActionPressed(useLocalSpace))
        //     Gizmo.UseLocalSpace = !Gizmo.UseLocalSpace;
        // Prevent object picking if user is interacting with the gizmo
        if (Gizmo.Hovering || Gizmo.Editing)
            return;

        //Detect mouse click and handle selection
        if (@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed)
        {
            GD.Print("Mouse Click _UnHandledINput Detected: " + @event + "From: " + this.Name);
            HandleGizmoSelection(Gizmo, mouseButton);
        }
    }

    //TODO: I CAN VASNTLY SIMPLIFY THIS LOGIC BELOW> IT WORKS FOR MOST SCENEARIOS, but IN MY CASE< I KNOW IT WILL ALWAYS BE THE 
    //Model3DMainPivotControl as TargetNode.. So I can simple set that as Target and I can aavoid needing to Create Collisions.StaticBodys,etc

    private void HandleGizmoSelection(Gizmo3D gizmo, InputEventMouseButton button)
    {
        // Raycast from the camera
        Camera3D camera = GetViewport().GetCamera3D();
        Vector3 dir = camera.ProjectRayNormal(button.Position);
        Vector3 from = camera.ProjectRayOrigin(button.Position);
        var result = GetWorld3D().DirectSpaceState.IntersectRay(new PhysicsRayQueryParameters3D()
        {
            From = from,
            To = from + dir * 1000.0f
        });
        if (result.Count == 0)
        {
            gizmo.ClearSelection();
            return;
        }

        GD.Print("### Gizmo Logic => Model Click detected from : " + this.Name);

        Node collider = (Node)result["collider"];
        GD.Print("### Gizmo Logic => colliderNode Clicked : " + collider.Name);
        //Node3D targetNode = collider.GetParent<Node3D>().GetParent<Node3D>();


        Model3DMainPivotControl targetNode = GlobalUtil.GetFirstParentNodeByType<Model3DMainPivotControl>(collider, 4);//TODO TO a recursive search on all paarents until you get to Model3DMainPivotControl

        if (targetNode == null || targetNode.Name == "ShaderMeshes") return;

        GD.Print("### Gizmo Logic => TargetNode : " + targetNode.Name);

        if (hasNodeSelected)
        {

            if (gizmo.IsSelected(targetNode))
            {
                gizmo.Deselect(targetNode);
                return;
            }

            gizmo.ClearSelection();
            gizmo.Select(targetNode);
            return;
        }
        else
        {
            gizmo.Select(targetNode);
            return;
        }
        // if (!Gizmo.Deselect(node))
        // 	Gizmo.Select(node);
    }


}
