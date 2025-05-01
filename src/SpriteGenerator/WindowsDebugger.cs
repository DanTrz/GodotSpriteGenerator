using Godot;

public partial class WindowsDebugger : PanelContainer
{
    [Export] OptionButton WindowsReziseOptBtn;
    [Export] OptionButton ColumnCountOptionButton;
    [Export] BoxContainer OptionsListVBoxContainer;
    [Export] HSlider WindowRatioHSlider;

    [Export] Control ImgRenderViewportContainer;
    [Export] ScrollContainer OptionSrollContainer;
    [Export] Label HSliderValue;
    [Export] Label WinSizeValue;
    [Export] Label ScrollPanelSizeValue;

    public override void _Ready()
    {
        WindowsReziseOptBtn.ItemSelected += OnWindowsReziseOptBtnItemSelected;
        ColumnCountOptionButton.ItemSelected += OnColumnCountOptionButton;
        WindowRatioHSlider.ValueChanged += OnWindowRatioHSliderValueChanged;
        HSliderValue.Text = "1";
    }

    public override void _Process(double delta)
    {
        UpdateDeguggerValues();
    }

    private void UpdateDeguggerValues()
    {
        WinSizeValue.Text = GetViewport().GetWindow().Size.ToString();
        ScrollPanelSizeValue.Text = OptionSrollContainer.Size.ToString();
        HSliderValue.Text = ImgRenderViewportContainer.SizeFlagsStretchRatio.ToString();
    }

    private void OnWindowRatioHSliderValueChanged(double value)
    {
        ImgRenderViewportContainer.SizeFlagsStretchRatio = (float)value;

    }


    private void OnColumnCountOptionButton(long index)
    {
        // int colCount = ColumnCountOptionButton.GetItemText((int)index).ToInt();
        // GlobalEvents.Instance.OnSpriteGenOptionsResized?.Invoke(this, colCount);

    }


    private void OnWindowsReziseOptBtnItemSelected(long index)
    {
        //Vector2 minSize = new Vector2(1280, 720);
        Vector2 minSize = index switch
        {
            1 => new Vector2(1280, 720),
            2 => new Vector2(1920, 1080),
            3 => new Vector2(2560, 1440),
            4 => new Vector2(3840, 2160),
            _ => new Vector2(1280, 720)
        };

        //switch (index)
        //{
        //    case 0:
        //        break;
        //    case 1:
        //        minSize = new Vector2(1280, 720);
        //        break;
        //    case 2:
        //        minSize = new Vector2(1920, 1080);
        //        break;
        //    case 3:
        //        minSize = new Vector2(2560, 1440);
        //        break;
        //    case 4:
        //        minSize = new Vector2(3840, 2160);
        //        break;
        //}

        OptionsListVBoxContainer.CustomMinimumSize = new Vector2(minSize.X, minSize.Y);
    }

}
