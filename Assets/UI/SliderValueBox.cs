using System;
using Godot;

[GlobalClass]
public partial class SliderValueBox : HSlider
{

    private Label valueLabel;

    public override void _Ready()
    {
        valueLabel = GetNode<Label>("%ValueLabel");
        this.ValueChanged += UpdateValueLabel;
        valueLabel.Text = Value.ToString("0.0");
    }

    private void UpdateValueLabel(double newValue)
    {
        valueLabel.Text = newValue.ToString("0.0");
    }

}

