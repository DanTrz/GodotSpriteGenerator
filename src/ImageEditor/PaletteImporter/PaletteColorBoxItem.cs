using System;
using Godot;

public partial class PaletteColorBoxItem : PanelContainer
{

    private Color _paletteColor;
    [Export]
    public Color PaletteColor
    {
        get
        {
            return _paletteColor;
        }
        set
        {
            _paletteColor = value;
            SetPaletteColor();
        }
    }

    public override void _Ready()
    {
        if (PaletteColor == default(Color))
        {
            PaletteColor = Colors.Black;
        }

        this.AddThemeStyleboxOverride("panel", new StyleBoxFlat { BgColor = PaletteColor });
        //bg_color
    }

    public void SetPaletteColor()
    {
        this.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = PaletteColor
        });
    }


}
