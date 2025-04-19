using System;
using Godot;

public partial class PaletteListGrid : GridContainer
{

    [Export] PackedScene paletteColorItemScene;
    int _minColumnsSize = 14;
    int _maxColumnsSize = 70;


    public override void _Ready()
    {
        ClearGridList();
    }

    public void ChangeGridColumnSize(int panelColumns)
    {

        int newColSize = _minColumnsSize;
        switch (panelColumns)
        {
            case >= 5:
                newColSize = _maxColumnsSize;
                break;
            case >= 4:
                newColSize = 54;
                break;
            case >= 3:
                newColSize = 42;
                break;
            case >= 2:
                newColSize = 30;
                break;
            case >= 1:
                newColSize = 14;
                break;

            default:
                break;
        }


        this.Columns = Math.Clamp(newColSize, _minColumnsSize, _maxColumnsSize);
        Log.Debug($"{this.Name} Inside: {GetOwner().Name} Changed Col to: {this.Columns}");




        // if (panelColumns >= 5)
        // {
        //     this.Columns = this.Columns * panelColumns;
        // }



    }

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
