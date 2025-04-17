using System;
using Godot;

[Tool]
public partial class ShowHideButton : Button
{
    [Export] string OpenedText = "Open";
    [Export] string ClosedText = "Close";
    [Export] Godot.Collections.Array<Control> TargetControlNodes;

    [Export] bool UpdateTextToLabel = false;
    [Export] Label TargetLabel;


    public override void _Ready()
    {
        this.Pressed += TogglePanel;

#if TOOLS
        if (!Engine.IsEditorHint())
        {
            TogglePanel();
        }
#endif
    }

    private void TogglePanel()
    {
        foreach (var controlNode in TargetControlNodes)
        {
            controlNode.Visible = !controlNode.Visible;
            if (UpdateTextToLabel)
            {
                if (TargetLabel != null)
                {
                    TargetLabel.Text = controlNode.Visible ? OpenedText : ClosedText;
                }
            }
            else
            {
                this.Text = controlNode.Visible ? OpenedText : ClosedText;
            }
        }
    }
}
