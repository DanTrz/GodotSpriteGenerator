using System;
using Godot;

[GlobalClass]
public partial class SliderValueBox : HSlider
{


    [Export] private Label valueLabel;

    public override void _Ready()
    {
        this.ValueChanged += UpdateValueLabel;
        UpdateValueLabel(Value);

    }

    private void UpdateValueLabel(double newValue)
    {
        if (valueLabel != null)
        {
            valueLabel.Text = newValue.ToString("0.00");
        }
    }

}

