using System;
using Godot;

public partial class DisplayViewportContainer : SubViewportContainer
{
    [Export] CamSpriteGenController camera2D;
    [Export] ModelScene3d MainModelScene;

    private Vector2 _defaultPosition;
    private bool _isPanning = false;
    private Vector2 _previousMousePosition;


    public override void _UnhandledInput(InputEvent @inputEvent)
    {
        if (@inputEvent is InputEventMouseButton mouseButton)
        {
            Log.Debug(this, "Unhandled mouseButtonInput Event: " + @inputEvent);
        }

    }

    public override void _GuiInput(InputEvent @inputEvent)
    {

        if (@inputEvent is InputEventMouseMotion motionEvent && _isPanning)
        {
            camera2D.PanViewToMousePos(motionEvent);
        }

        if (@inputEvent is InputEventMouseButton mouseButton)
        {
            if (@inputEvent is InputEventMouseButton mouseEvent)
            {
                if (mouseEvent.ButtonIndex == MouseButton.Right && mouseEvent.Pressed)
                {
                    _isPanning = true;
                    _previousMousePosition = GetGlobalMousePosition();
                }
                else if ((mouseEvent.ButtonIndex == MouseButton.WheelUp || mouseEvent.ButtonIndex == MouseButton.WheelDown) && mouseEvent.Pressed)
                {
                    camera2D.ZoomTowardsMouse(!(mouseEvent.ButtonIndex == MouseButton.WheelUp));

                }
                else
                {
                    _isPanning = false;
                }
            }

        }
    }


}
