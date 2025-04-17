using System;
using System.Linq;
using Gizmo3DPlugin;
using Godot;

public partial class ModelScene3d : Node3D
{ //public Node3D MainMesh;

    [Export] public Gizmo3D Gizmo { get; private set; }
    bool hasNodeSelected => Gizmo.Selections.Count > 0;
    private bool _isPanning = false;

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
            //Log.Debug("Mouse Click _UnHandledINput Detected: " + @event + "From: " + this.Name);
            HandleGizmoSelection(Gizmo, mouseButton);
        }

        //Detect mouse WheelMovement (Zoom) and handle it
        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.WheelUp && mouseEvent.Pressed)
            {
                //Log.Debug("WheelUp Detected " + this.Name);
                //Zoom Out ////bool True = Zoom In, False = Zoom Out
                GlobalEvents.Instance.OnCameraZoomChanged?.Invoke(false);

            }
            else if (mouseEvent.ButtonIndex == MouseButton.WheelDown && mouseEvent.Pressed)
            {
                //Log.Debug("WheelDown Detected " + this.Name);
                GlobalEvents.Instance.OnCameraZoomChanged?.Invoke(true);
                //Zoom In ////bool True = Zoom In, False = Zoom Out
            }


        }
        else if (@event is InputEventMouseMotion motionEvent && _isPanning)
        {

            //Log.Debug("Scroll Requested = " + motionEvent.Relative + "  From: " + this.Name);
            GlobalEvents.Instance.OnPaningScroll?.Invoke(motionEvent);
            GetViewport().SetInputAsHandled();
        }
    }

    private void HandleGizmoSelection(Gizmo3D gizmo, InputEventMouseButton button)
    {

        //Log.Debug("### Gizmo Logic => Click detected : " + this.Name);
        var targetNode = GlobalUtil.GetAllChildNodesByType<Model3DMainPivotControl>(this).FirstOrDefault();
        if (targetNode == null) return;
        //Log.Debug("### Gizmo Logic => TargetNode : " + targetNode.Name);

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
        var pivotNode = GlobalUtil.GetAllChildNodesByType<Model3DMainPivotControl>(this).FirstOrDefault();
        var allChildMesheInstance3D = GlobalUtil.GetAllChildNodesByType<MeshInstance3D>(pivotNode);
        foreach (var meshInstance3dObj in allChildMesheInstance3D)
        {
            var allCollisionShapes = GlobalUtil.GetAllChildNodesByType<CollisionShape3D>(meshInstance3dObj);
            if (allCollisionShapes.Count == 0)
            {
                meshInstance3dObj.CreateConvexCollision();
                Log.Debug($"Adding ConvexCollisionto: {meshInstance3dObj.Name}");
            }

        }
    }

}
