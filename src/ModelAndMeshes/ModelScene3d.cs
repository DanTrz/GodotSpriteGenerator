using System;
using System.Linq;
using Gizmo3DPlugin;
using Godot;

public partial class ModelScene3d : Node3D
{ //public Node3D MainMesh;

    [Export] public Gizmo3D Gizmo { get; private set; }
    bool hasNodeSelected => Gizmo.Selections.Count > 0;

    public override void _Ready()
    {
        Gizmo.TransformChanged += (sender, args) => GlobalEvents.Instance.OnModelTransformChanged?.Invoke(1, new Vector3(0, 0, 0));
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (Gizmo.Hovering || Gizmo.Editing)
            return;

        //Detect mouse click and handle selection
        if (@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed)
        {
            //GD.Print("Mouse Click _UnHandledINput Detected: " + @event + "From: " + this.Name);
            HandleGizmoSelection(Gizmo, mouseButton);
        }

        //Detect mouse WheelMovement (Zoom) and handle it
        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.WheelUp && mouseEvent.Pressed)
            {
                //Zoom Out ////bool True = Zoom In, False = Zoom Out
                GlobalEvents.Instance.OnCameraZoomChanged?.Invoke(false);

            }
            else if (mouseEvent.ButtonIndex == MouseButton.WheelDown && mouseEvent.Pressed)
            {
                GlobalEvents.Instance.OnCameraZoomChanged?.Invoke(true);
                //Zoom In ////bool True = Zoom In, False = Zoom Out
            }
        }
    }

    private void HandleGizmoSelection(Gizmo3D gizmo, InputEventMouseButton button)
    {

        //GD.Print("### Gizmo Logic => Click detected : " + this.Name);
        var targetNode = GlobalUtil.GetAllChildNodesByType<Model3DMainPivotControl>(this).FirstOrDefault();
        if (targetNode == null) return;
        //GD.Print("### Gizmo Logic => TargetNode : " + targetNode.Name);

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


    // private void HandleGizmoSelectionLegacy(Gizmo3D gizmo, InputEventMouseButton button)
    // {
    //     // Raycast from the camera
    //     Camera3D camera = GetViewport().GetCamera3D();
    //     Vector3 dir = camera.ProjectRayNormal(button.Position);
    //     Vector3 from = camera.ProjectRayOrigin(button.Position);
    //     var result = GetWorld3D().DirectSpaceState.IntersectRay(new PhysicsRayQueryParameters3D()
    //     {
    //         From = from,
    //         To = from + dir * 1000.0f
    //     });
    //     if (result.Count == 0)
    //     {
    //         gizmo.ClearSelection();
    //         return;
    //     }

    //     GD.Print("### Gizmo Logic => Model Click detected from : " + this.Name);

    //     Node collider = (Node)result["collider"];
    //     GD.Print("### Gizmo Logic => colliderNode Clicked : " + collider.Name);
    //     //Node3D targetNode = collider.GetParent<Node3D>().GetParent<Node3D>();


    //     Model3DMainPivotControl targetNode = GlobalUtil.GetFirstParentNodeByType<Model3DMainPivotControl>(collider, 4);//TODO TO a recursive search on all paarents until you get to Model3DMainPivotControl

    //     if (targetNode == null || targetNode.Name == "ShaderMeshes") return;

    //     GD.Print("### Gizmo Logic => TargetNode : " + targetNode.Name);

    //     if (hasNodeSelected)
    //     {

    //         if (gizmo.IsSelected(targetNode))
    //         {
    //             gizmo.Deselect(targetNode);
    //             return;
    //         }

    //         gizmo.ClearSelection();
    //         gizmo.Select(targetNode);
    //         return;
    //     }
    //     else
    //     {
    //         gizmo.Select(targetNode);
    //         return;
    //     }
    //     // if (!Gizmo.Deselect(node))
    //     // 	Gizmo.Select(node);
    // }





}
