using Godot;

[GlobalClass]
public partial class TransformUiGroup : HBoxContainer
{

    [Export] public string MainLabelName;
    [Export] public Label MainGroupLabel;
    [Export] public Const.TransformType TransformType = Const.TransformType.POSITION;

    [Export] public LineEdit XAxisLabelLineTextEdit;
    [Export] public LineEdit YAxisLabelLineTextEdit;
    [Export] public LineEdit ZAxisLabelLineTextEdit;

    public override void _Ready()
    {
        MainGroupLabel.Text = MainLabelName;
    }
}
