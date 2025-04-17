using System;
using Godot;

public partial class ShortcurtsSpriteGenPanel : PanelContainer
{
    [Export] private Button ResetCamButton;

    public override void _Ready()
    {
        ResetCamButton.Pressed += () => GlobalEvents.Instance.OnCamResetChanges?.Invoke();
    }

    private void OnResetCamButtonPressed()
    {

    }
}
