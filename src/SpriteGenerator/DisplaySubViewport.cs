using System;
using Godot;

public partial class DisplaySubViewport : SubViewport
{
    //TEST //BUG - Entire script is test and can delete
    public override void _UnhandledInput(InputEvent @event)
    {
        //Log.Debug($"UnhandledInput from {this.Name}");

        if (Input.IsActionPressed(Const.MOUSE_RIGHT_CLICK))
        {
            // Log.Debug($"MouseRightClick from {this.Name}");
        }

    }
}
