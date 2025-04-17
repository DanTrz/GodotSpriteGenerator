using System;
using System.Linq;
using Gizmo3DPlugin;
using Godot;

public partial class Model3DMainPivotControl : Node3D
{
    //     //public Node3D MainMesh;

    //     [Export] public Gizmo3D Gizmo { get; private set; }
    //     bool hasNodeSelected => Gizmo.Selections.Count > 0;
    //     // [Export] public float RotationSpeed = 0.01f;
    //     // [Export] public float VerticalAngleLimit = 180.0f;  // Limit vertical angle to prevent flipping.
    //     // [Export] public float scrollSpeed = 1f;

    //     // private float _horizontalAngle = 0.0f;
    //     // private float _verticalAngle = 0.0f;
    //     // private const float Rad2Deg = 180f / Mathf.Pi;  // Define Rad2Deg manually.


    //     public override void _Ready()
    //     {
    //         //       MainMesh = GetChild<Node3D>(0);

    //         GlobalEvents.Instance.OnSpriteGeneratorMouseClick += OnSpriteGeneratorMouseClick;
    //         Gizmo.TransformChanged += OnGizmoTargetTransformChanged;
    //         AddChildrenStaticBodyCollisionShape();
    //     }

    //     private void OnGizmoTargetTransformChanged(int mode, Vector3 value)
    //     {
    //         //Mode 1 = Rotation // Mode 2 = Position // Mode 3 = Scale
    //         Log.Debug($"GismoTransform ended value: {value} and mode: {mode}");
    //         //TODO: Update the Node ModelManager to sync the ModelPosition and ModelRotation
    //     }


    //     private void OnSpriteGeneratorMouseClick(InputEventMouseButton mouseButton)
    //     { //
    //         HandleGizmoSelection(Gizmo, mouseButton);
    //     }

    //     private void AddChildrenStaticBodyCollisionShape()
    //     {
    //         var allChildMesheInstance3D = GlobalUtil.GetAllNodesByType<MeshInstance3D>(this);
    //         foreach (var meshInstance3dObj in allChildMesheInstance3D)
    //         {
    //             var allCollisionShapes = GlobalUtil.GetAllNodesByType<CollisionShape3D>(meshInstance3dObj);
    //             if (allCollisionShapes.Count == 0)
    //             {
    //                 meshInstance3dObj.CreateConvexCollision();
    //                 Log.Debug($"Adding ConvexCollisionto: {meshInstance3dObj.Name}");
    //             }

    //         }
    //     }



    //     // public override void _Input(InputEvent @event)
    //     // {
    //     //     if (@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed)
    //     //     {
    //     //         Log.Debug("Mouse Click _Input Detected: " + @event + "From: " + this.Name);
    //     //         //HandleGizmoSelection(Gizmo, mouseButton);
    //     //     }
    //     // }


    //     public override void _UnhandledInput(InputEvent @event)
    //     {
    //         //Log.Debug("_UnhandledInput Detected: " + @event + "From: " + this.Name);
    //         // Swap gizmo with custom gizmo or vice versa
    //         // if (@event.IsActionPressed(customGizmo))
    //         // {
    //         // 	Node parent = Gizmo.GetParent();
    //         // 	int index = Gizmo.GetIndex();
    //         // 	Gizmo.QueueFree();
    //         // 	Gizmo = Gizmo is CustomGizmo ? new Gizmo3D() : new CustomGizmo();
    //         // 	//Camera.Gizmo = Gizmo;
    //         // 	parent.AddChild(Gizmo);
    //         // 	parent.MoveChild(Gizmo, index);
    //         // 	CustomLabel.Text = Gizmo is CustomGizmo ? "Default Gizmo: G" : "Custom Gizmo: G";
    //         // }

    //         // Enable/Disable Toggle modes
    //         // if (@event.IsActionPressed(moveMode))
    //         // 	Gizmo.Mode ^= Gizmo3D.ToolMode.Move;
    //         // if (@event.IsActionPressed(scaleMode))
    //         // 	Gizmo.Mode ^= Gizmo3D.ToolMode.Scale;
    //         // if (@event.IsActionPressed(rotateMode))
    //         // 	Gizmo.Mode ^= Gizmo3D.ToolMode.Rotate;

    //         // // Toggle between local and global space
    //         // if (!Gizmo.Editing && @event.IsActionPressed(useLocalSpace))
    //         //     Gizmo.UseLocalSpace = !Gizmo.UseLocalSpace;
    //         // Prevent object picking if user is interacting with the gizmo
    //         if (Gizmo.Hovering || Gizmo.Editing)
    //             return;

    //         //Detect mouse click and handle selection
    //         if (@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed)
    //         {
    //             Log.Debug("Mouse Click _UnHandledINput Detected: " + @event + "From: " + this.Name);
    //             HandleGizmoSelection(Gizmo, mouseButton);
    //         }
    //     }

    //     private void HandleGizmoSelection(Gizmo3D gizmo, InputEventMouseButton button)
    //     {
    //         // Raycast from the camera
    //         Camera3D camera = GetViewport().GetCamera3D();
    //         Vector3 dir = camera.ProjectRayNormal(button.Position);
    //         Vector3 from = camera.ProjectRayOrigin(button.Position);
    //         var result = GetWorld3D().DirectSpaceState.IntersectRay(new PhysicsRayQueryParameters3D()
    //         {
    //             From = from,
    //             To = from + dir * 1000.0f
    //         });
    //         if (result.Count == 0)
    //         {
    //             gizmo.ClearSelection();
    //             return;
    //         }

    //         Log.Debug("### Gizmo Logic => Model Click detected from : " + this.Name);

    //         Node collider = (Node)result["collider"];
    //         //Node3D targetNode = collider.GetParent<Node3D>().GetParent<Node3D>();
    //         Node3D targetNode = collider.GetParent<Node3D>().GetParent<Node3D>();//TODO TO a recursive search on all paarents until you get to Model3DMainPivotControl

    //         Log.Debug("### Gizmo Logic => TargetNode : " + targetNode.Name);

    //         if (hasNodeSelected)
    //         {

    //             if (gizmo.IsSelected(targetNode))
    //             {
    //                 gizmo.Deselect(targetNode);
    //                 return;
    //             }

    //             gizmo.ClearSelection();
    //             gizmo.Select(targetNode);
    //             return;
    //         }
    //         else
    //         {
    //             gizmo.Select(targetNode);
    //             return;
    //         }
    //         // if (!Gizmo.Deselect(node))
    //         // 	Gizmo.Select(node);
    //     }


}
