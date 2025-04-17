using System;
using Godot;

public partial class AutoGrid : Control
{
    [Export] public int GridSize = 32;
    [Export] public Color LineColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    public override void _Ready()
    {
        // Only redraw when resized
        SetProcess(false);
        SetNotifyTransform(true);
    }

    public override void _Draw()
    {

        {
            var size = GetRect().Size;
            var width = (int)size.X;
            var height = (int)size.Y;

            for (int x = 0; x <= width; x += GridSize)
            {
                DrawLine(new Vector2(x, 0), new Vector2(x, height), LineColor);
            }

            for (int y = 0; y <= height; y += GridSize)
            {
                DrawLine(new Vector2(0, y), new Vector2(width, y), LineColor);
            }
        }
    }


    public override void _Notification(int what)
    {
        if (what == NotificationResized)
        {
            QueueRedraw();
        }
    }
}

