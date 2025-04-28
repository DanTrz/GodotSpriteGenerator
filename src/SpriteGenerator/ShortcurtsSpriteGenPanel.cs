using Godot;

public partial class ShortcurtsSpriteGenPanel : PanelContainer
{
    [Export] private Button ResetViewButton;



    public override void _Ready()
    {
        ResetViewButton.Pressed += () => GlobalEvents.Instance.OnCamResetChanges?.Invoke();
        ResetViewButton.Pressed += TestDebugger;
    }
}





