using System;
using Godot;

public partial class PixelSmoothEffectPanel : PanelContainer
{
    public override void _Ready()
    {
        // this.GuiInput += OnGuiInput;
    }

    // private void OnGuiInput(InputEvent @event)
    // {
    //     //Log.Debug("_GuiInput Detected: " + @event + "From: " + this.Name);

    //     if (@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed)
    //     {
    //         Log.Debug("Mouse Click _Input Detected: " + @event + "From: " + this.Name);
    //         GlobalEvents.Instance.OnSpriteGeneratorMouseClick?.Invoke(mouseButton);
    //         //HandleGizmoSelection(Gizmo, mouseButton);
    //     }
    // }


    // public override void _UnhandledInput(InputEvent @event)
    // {
    //     Log.Debug("_UnhandledInput Detected: " + @event + "From: " + this.Name);
    // }

    // public override void _Input(InputEvent @event)
    // {
    //     if (@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed)
    //     {
    //         Log.Debug("Mouse Click _Input Detected: " + @event + "From: " + this.Name);
    //         //HandleGizmoSelection(Gizmo, mouseButton);
    //     }
    // }



}
