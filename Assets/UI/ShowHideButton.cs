using System;
using Godot;

public partial class ShowHideButton : Button
{
    [Export] string OpenedText = "Close";
    [Export] string ClosedText = "Open";
    [Export] bool StartOpen = false;
    [Export] public Control TargetControlNode;

    [Export] bool UpdateTextToLabel = false;
    [Export] Label TargetLabel;


    public override void _Ready()
    {
        this.Pressed += TogglePanel;

        if (!Engine.IsEditorHint())
        {
            TogglePanel(StartOpen);
        }
    }

    private void TogglePanel()
    {
        TargetControlNode.Visible = !TargetControlNode.Visible;
        UpdateLabelText(TargetControlNode);
    }

    private void TogglePanel(bool startOpen)
    {
        TargetControlNode.Visible = startOpen;
        UpdateLabelText(TargetControlNode);
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
