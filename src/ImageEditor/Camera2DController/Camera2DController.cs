using System;
using Godot;

public partial class Camera2DController : Camera2D
{

    [Export] public float ZoomSpeed = 0.1f;
    [Export] public float PanSpeed = 750.0f;  // No longer directly used, but kept for reference
    [Export] public Vector2 DefaultZoom = new Vector2(0.5f, 0.5f);
    [Export] public Vector2 MinZoom = new Vector2(0.10f, 0.10f);
    [Export] public Vector2 MaxZoom = new Vector2(6.0f, 6.0f);

    private Vector2 _defaultPosition;
    private bool _isPanning = false;
    private Vector2 _previousMousePosition; // Store previous mouse position


    public override void _Ready()
    {
        _defaultPosition = Position;
        MakeCurrent();
        Zoom = DefaultZoom;

    }

    public override void _UnhandledInput(InputEvent @event)
    {
        //var parentControlNode = GetParent().GetParent<SubViewportContainer>();

        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.WheelUp && mouseEvent.Pressed)
            {
                if (Zoom.X <= (MinZoom.X + 0.01)) return;
                Zoom -= new Vector2(ZoomSpeed, ZoomSpeed);
                Zoom = Zoom.Clamp(MinZoom, MaxZoom);
                GetViewport().SetInputAsHandled();
            }
            else if (mouseEvent.ButtonIndex == MouseButton.WheelDown && mouseEvent.Pressed)
            {
                Zoom += new Vector2(ZoomSpeed, ZoomSpeed);
                Zoom = Zoom.Clamp(MinZoom, MaxZoom);
                GetViewport().SetInputAsHandled();
            }
            else if (mouseEvent.ButtonIndex == MouseButton.Right && mouseEvent.Pressed)
            {
                _isPanning = true;
                _previousMousePosition = GetGlobalMousePosition(); // Store initial position
                                                                   // Change cursor to grab hand
                GetViewport().SetInputAsHandled();
            }
            //else if (mouseEvent.ButtonIndex == MouseButton.Right && !mouseEvent.Pressed)
            else
            {

                _isPanning = false;
                // Restore cursor
                GetViewport().SetInputAsHandled();
            }
        }
        else if (@event is InputEventMouseMotion motionEvent && _isPanning)
        {
            // Use the *relative* mouse movement from the event
            Position -= motionEvent.Relative / Zoom;  // Divide by Zoom for correct scaling
            Log.Debug($" New CamPosition {Position} PanMouse= " + motionEvent.Relative + "  From: " + this.GetOwner().Name);
            GetViewport().SetInputAsHandled();
        }

    }

    public void ResetCamera()
    {
        Position = _defaultPosition;
        Zoom = DefaultZoom;
    }
}
