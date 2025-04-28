using Godot;

public partial class ItemListCheckBox : ItemList
{

    [Export] public Texture2D ICON_SELECTED;
    [Export] public Texture2D ICON_UNSELECTED;


    public override void _Ready()
    {
        this.MultiSelected += OnSelectionChangedOrUpdated;

        UpdateAllItemsIcon();

    }

    public void CreateItemsFromList(string[] itemNames)
    {
        foreach (var item in itemNames)
        {
            AddItem(item, ICON_UNSELECTED, true);
        }
    }

    private void UpdateAllItemsIcon()
    {

        for (int i = 0; i < ItemCount; i++)
        {
            this.SetItemIcon(i, ICON_UNSELECTED);
        }

        foreach (var itemIndex in GetSelectedItems())
        {
            UpdateSingleItemIcon(itemIndex, true);
        }

    }


    /// <summary>
    /// Called when the selection changes or is updated.
    /// </summary>
    /// <param name="index">Index of the item that was selected or deselected.</param>
    private void OnSelectionChangedOrUpdated(long index, bool selected)
    {
        //Log.Debug("ItemClicked: " + this.GetItemText((int)index) + " + index: " + index + " selected: " + selected);
        UpdateSingleItemIcon((int)index, selected);


    }

    private void UpdateSingleItemIcon(int index, bool selected)
    {
        if (selected)
        {
            SetItemIcon(index, ICON_SELECTED);
        }
        else
        {
            SetItemIcon(index, ICON_UNSELECTED);
        }
    }



}
