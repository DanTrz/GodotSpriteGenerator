using System;
using Godot;

[GlobalClass]
public partial class GridPanelContainer : GridContainer
{
    [Export] bool _useSingleRowView = false;
    [Export] int _minColCount = 1;
    [Export] int _maxColCount = 10;

    public void ChangeGridColumnSize(int newColCount)
    {
        if (_useSingleRowView)
        {
            _minColCount = 3;
        }

        this.Columns = Math.Clamp(newColCount, _minColCount, _maxColCount);
        Log.Debug(this, $"Owner:{GetOwner().Name} NewColSize:{this.Columns}");



    }




}
