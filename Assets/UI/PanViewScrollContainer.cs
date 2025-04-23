using System;
using Godot;

public partial class PanViewScrollContainer : ScrollContainer
{
    //     private bool _isPanning = false;

    [Export] public int GridSize = 32;
    [Export] public Color LineColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    public override void _Ready()
    {

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


}