using Godot;

public partial class ModelPositionManager : Node
{

    [Export] public float MoveSpeed = 0.1f;
    [Export] public float RotateSpeed = 1.0f;
    [Export] public float ZoomSpeed = 0.1f;

    [OnReady("%MoveLeftBtn")] private Button _moveLeftBtn;
    [OnReady("%MoveRightBtn")] private Button _moveRightBtn;
    [OnReady("%MoveUpBtn")] private Button _moveUpBtn;
    [OnReady("%MoveDownBtn")] private Button _moveDownBtn;
    [OnReady("%RotateXAxisBtn")] private Button _rotateXAxisBtn;
    [OnReady("%RotateYAxisBtn")] private Button _rotateYAxisBtn;
    [OnReady("%ZoomInBtn")] private Button _zoomInBtn;
    [OnReady("%ZoomOutBtn")] private Button _zoomOutBtn;

    public Node3D ModelNode;
    public Camera3D CameraNode;
    private bool _isModeLeftBtnHeld = false;

    public override void _Ready()
    {
        _moveLeftBtn.Pressed += MoveModelLeft;
        _moveRightBtn.Pressed += MoveModelRight;
        _moveUpBtn.Pressed += MoveModeUp;
        _moveDownBtn.Pressed += MoveModeDown;

        _rotateXAxisBtn.Pressed += RotateModeXAxis;
        _rotateYAxisBtn.Pressed += RotateModeYAxis;

        _zoomInBtn.Pressed += ZoomModelIn;
        _zoomOutBtn.Pressed += ZoomModelOut;


        //Test for future code to track button being held down
        // _moveLeftBtn.ButtonDown += () => { _isModeLeftBtnHeld = true; };
        // _moveLeftBtn.ButtonUp += () => { _isModeLeftBtnHeld = false; };

        //Check if the ModelNode is not null
        this.CallDeferred(MethodName.CheckIfModelLoaded);

    }

    private void ZoomModelOut()
    {
        CameraNode.Position += new Vector3(0, 0, ZoomSpeed);
    }


    private void ZoomModelIn()
    {
        CameraNode.Position += new Vector3(0, 0, -ZoomSpeed);
    }


    public override void _Process(double delta)
    {
        if (_moveLeftBtn.IsPressed())
        {
            //GD.Print("Button Left move is being held down!");
            MoveModelLeft();
        }

        if (_rotateXAxisBtn.IsPressed())
        {
            RotateModeXAxis();
        }

        if (_rotateYAxisBtn.IsPressed())
        {

            RotateModeYAxis();
        }
    }

    private void MoveModelLeft()
    {
        ModelNode.Position += new Vector3(-MoveSpeed, 0, 0);
    }

    private void MoveModelRight()
    {
        ModelNode.Position += new Vector3(MoveSpeed, 0, 0);
    }

    private void MoveModeUp()
    {
        ModelNode.Position += new Vector3(0, MoveSpeed, 0);
    }

    private void MoveModeDown()
    {
        ModelNode.Position += new Vector3(0, -MoveSpeed, 0);
    }

    private void RotateModeXAxis()
    {
        ModelNode.RotationDegrees += new Vector3(RotateSpeed, 0, 0);
    }

    private void RotateModeYAxis()
    {
        ModelNode.RotationDegrees += new Vector3(0, RotateSpeed, 0);
    }

    public void CheckIfModelLoaded()
    {
        if (ModelNode == null || CameraNode == null)
        {
            GD.PrintErr("Model or Camera in ModelPositionManager is null");
        }
    }
}
