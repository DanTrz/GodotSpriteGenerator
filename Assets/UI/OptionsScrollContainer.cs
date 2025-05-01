using Godot;
using System.Collections.Generic;

public partial class OptionsScrollContainer : ScrollContainer
{
    public override void _Ready()
    {
        this.Resized += OnOptScrollContainerResized;
    }

    private void OnOptScrollContainerResized()
    {
        //Log.Info($"{this.Name} Resized to: {this.Size}");

        Vector2 containerSize = this.Size;

        int columns = containerSize.X switch
        {
            >= 1714 => 5,
            >= 1426 => 4,
            >= 1102 => 3,
            >= 740 => 2,
            _ => 1
        };

        //switch (containerSize.X)
        //{
        //    case >= 1714:
        //        columns = 5;
        //        break;
        //    case >= 1426:
        //        columns = 4;
        //        break;
        //    case >= 1102:
        //        columns = 3;
        //        break;
        //    case >= 740:
        //        columns = 2;
        //        break;
        //    default:
        //        columns = 1;
        //        break;
        //}

        if (columns > 0)
        {

            var GridPanelList = GlobalUtil.GetAllChildNodesByType<GridPanelContainer>(this);

            if (GridPanelList != null && GridPanelList.Count > 0)
            {
                ResizeChildGridPanels(GridPanelList, columns);
            }
            else
            {
                Log.Error($"{this.Name} has no GridPanelContainer or None was found");
            }

        }

    }

    private void ResizeChildGridPanels(List<GridPanelContainer> gridList, int newColSize)
    {
        foreach (var grid in gridList)
        {
            grid.ChangeGridColumnSize(newColSize);
        }
    }

}

