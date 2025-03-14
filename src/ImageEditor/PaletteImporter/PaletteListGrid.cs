using System;
using Godot;

public partial class PaletteListGrid : GridContainer
{
    [Export] PackedScene paletteColorItemScene;
    public void ClearGridList()
    {
        foreach (var gridItem in GetChildren())
        {
            RemoveChild(gridItem);
            gridItem.QueueFree();
        }
    }

    public void AddGridItem(Color paletteColor)
    {
        PaletteColorBoxItem paletteColorItem = paletteColorItemScene.Instantiate<PaletteColorBoxItem>();
        AddChild(paletteColorItem);
        paletteColorItem.PaletteColor = paletteColor;
    }
}
