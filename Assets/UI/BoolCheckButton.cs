using System;
using Godot;

public partial class BoolCheckButton : CheckButton
{
    public override void _Ready()
    {
        UpdateButtonText();
        this.Pressed += UpdateButtonText;

    }

    private void UpdateButtonText()
    {
        this.Text = this.ButtonPressed.ToString();
    }
}
