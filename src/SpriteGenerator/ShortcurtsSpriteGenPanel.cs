using Godot;

public partial class ShortcurtsSpriteGenPanel : PanelContainer
{
    [Export] private Button ResetViewButton;



    public override void _Ready()
    {
        ResetViewButton.Pressed += () => GlobalEvents.Instance.OnCamResetChanges?.Invoke();
        ResetViewButton.Pressed += TestDebugger;
    }

    private void TestDebugger()
    {
        Log.Debug(this, "Logged as Log.DEBUG Visual Studio");
        Log.Info(this, "Logged as Log.INFO Visual Studio");
        Log.Error(this, "Logged as Log.ERROR Visual Studio");
        Log.Warning(this, "Logged as Log.WARNING Visual Studio");
    }
}





