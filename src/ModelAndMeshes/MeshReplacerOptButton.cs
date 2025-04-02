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
        //BUG - Not invoking the signal - This is beauce no Listener Subscriber exist, therefore the Action is NUll.
        // We need to MAKE SURE an Subcrisbiver exist (E.G, this is not null). This happens because in the MeshReplacer.cs, where I should subscribe to this signal, it's not calling the code to load the subscriber.
        //By addint ? we are making sure it's not null.
        GlobalEvents.Instance.OnMeshItemColorChanged?.Invoke(itemSelectedName, BodyPartType, color);
    }


    public void EnableColorPicker(bool enabled)
    {
        MeshColorPickerMarginContainer.Visible = enabled;
    }
}
