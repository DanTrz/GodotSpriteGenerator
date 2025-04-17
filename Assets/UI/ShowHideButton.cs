using System;
using Godot;

[Tool]
public partial class ShowHideButton : Button
{
    [Export] string OpenedText = "Open";
    [Export] string ClosedText = "Close";
    [Export] bool StartOpen = false;
    [Export] Godot.Collections.Array<Control> TargetControlNodes;

    [Export] bool UpdateTextToLabel = false;
    [Export] Label TargetLabel;


    public override void _Ready()
    {
        this.Pressed += TogglePanel;

#if TOOLS
        if (!Engine.IsEditorHint())
        {
            TogglePanel(StartOpen);
        }
#endif
    }

    private void TogglePanel()
    {
        if (TargetControlNodes != null)
        {
            foreach (var controlNode in TargetControlNodes)
            {
                controlNode.Visible = !controlNode.Visible;
                UpdateLabelText(controlNode);
            }
        }
    }

    private void TogglePanel(bool startOpen)
    {
        if (TargetControlNodes != null)
        {
            foreach (var controlNode in TargetControlNodes)
            {
                controlNode.Visible = startOpen;
                UpdateLabelText(controlNode);
            }
        }
    }

    private void UpdateLabelText(Control controlNode)
    {
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
