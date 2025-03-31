using System;
using Godot;
using Godot.Collections;

public partial class MeshReplacerOptButton : OptionButton
{
    [Export] public Const.BodyPartType BodyPartType = Const.BodyPartType.HEAD;
    [Export] public ColorPickerButton MeshColorPicker;
    [Export] public MarginContainer MeshColorPickerMarginContainer;


    public override void _Ready()
    {
        EnableColorPicker(false);
        MeshColorPicker.ColorChanged += OnColorChanged;
    }

    private void OnColorChanged(Color color)
    {
        string itemSelectedName = this.GetItemText((int)this.Selected);

        //TODO: BUG this is not working - Not invoking the signal
        //BUG - Not invoking the signal
        GlobalEvents.Instance.OnMeshItemColorChanged.Invoke(itemSelectedName, color);
        //GlobalEvents.OnMeshItemColorChanged2.Invoke(itemSelectedName, color);

        //r(string itemSelectedName, int itemIndex, Color newColor)
        // EmitSignal("color_changed", BodyPartType, color);
    }

    public void EnableColorPicker(bool enabled)
    {
        MeshColorPickerMarginContainer.Visible = enabled;
    }
}
