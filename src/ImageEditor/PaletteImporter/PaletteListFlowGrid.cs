using System;
using Godot;

public partial class PaletteListFlowGrid : FlowContainer
{
    [Export] PackedScene paletteColorItemScene;

    public override void _Ready()
    {
        ClearFlowList();
    }

    public void ClearFlowList()
    {
        foreach (var child in GetChildren())
        {
            child.QueueFree();
        }
    }

    public void AddFlowGridItem(Color paletteColor)
    {
        PaletteColorBoxItem paletteColorItem = paletteColorItemScene.Instantiate<PaletteColorBoxItem>();
        AddChild(paletteColorItem);
        paletteColorItem.PaletteColor = paletteColor;
    }
}
