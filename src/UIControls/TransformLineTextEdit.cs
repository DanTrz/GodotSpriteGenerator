using Godot;

public partial class TransformLineTextEdit : LineEdit
{
    [Export] public Const.TransformAxis axis = Const.TransformAxis.Z;
}
