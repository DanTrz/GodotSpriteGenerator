using System;
using Godot;

public partial class ModelPositionManager : Node
{


    [Export] public LineEdit _camXRotationLineTextEdit;

    [Export] public LineEdit CamDistancelLineTextEdit;

    [Export] public LineEdit _posXAxisLineTextEdit;
    [Export] public LineEdit _posYAxisLineTextEdit;
    [Export] public LineEdit _posZAxisLineTextEdit;

    [Export] public LineEdit _rotationXAxisLineTextEdit;
    [Export] public LineEdit _rotationYAxisLineTextEdit;
    [Export] public LineEdit _rotationZAxisLineTextEdit;

    public Node3D ModelNode;
    public Camera3D CameraNode;
    private bool _isModeLeftBtnHeld = false;

    public override void _Ready()
    {
        //Check if the ModelNode is not null
        this.CallDeferred(MethodName.CheckIfModelLoaded);

        GD.Print("LoadTransformValueToUI being called");
        this.CallDeferred(MethodName.LoadTransformValueToUI);
        this.CallDeferred(MethodName.ConnectTransformUINodeSignals);

        //_moveLeftBtn.Pressed += MoveModelLeft;
        //_moveRightBtn.Pressed += MoveModelRight;
        //_moveUpBtn.Pressed += MoveModeUp;
        //_moveDownBtn.Pressed += MoveModeDown;

        //_rotateXAxisBtn.Pressed += RotateModeXAxis;
        //_rotateYAxisBtn.Pressed += RotateModeYAxis;

        //_zoomInBtn.Pressed += ZoomModelIn;
        //_zoomOutBtn.Pressed += ZoomModelOut;


        //Test for future code to track button being held down
        // _moveLeftBtn.ButtonDown += () => { _isModeLeftBtnHeld = true; };
        // _moveLeftBtn.ButtonUp += () => { _isModeLeftBtnHeld = false; };
    }

    public void CheckIfModelLoaded()
    {
        if (ModelNode == null || CameraNode == null)
        {
            GD.PrintErr("Model or Camera in ModelPositionManager is null");
        }
    }

    private void ConnectTransformUINodeSignals()
    {
        //GlobalUtil.GetAllNodesByType<LineEdit>(this).ForEach((transformlineEdit) =>
        //{
        //    transformlineEdit.TextChanged += (newValue) => OnTransformUIChanged(newValue, _zoomTransfNodeGroup, transformlineEdit);
        //});

        foreach (var node in GlobalUtil.GetAllNodesByType<LineEdit>(this))
        {
            if (node is LineEdit transformlineEdit)
            {
                //transformlineEdit.TextChanged += (newValue) => OnTransformUIChanged(newValue, transformlineEdit);
                transformlineEdit.TextChanged += OnTransformUIChanged;

            }
        }
    }

    private void OnTransformUIChanged(string newValue)
    {
        if (!String.IsNullOrEmpty(newValue) && !String.IsNullOrWhiteSpace(newValue) && float.TryParse(newValue, out float out_))
        {
            GD.PrintT("Valid Input: " + newValue);
            SetTransformValueToModel();
            //LoadTransformValueToUI();
        }
    }

    private void SetTransformValueToModel()
    {
        ModelNode.Position = new Vector3(float.Parse(_posXAxisLineTextEdit.Text), float.Parse(_posYAxisLineTextEdit.Text), float.Parse(_posZAxisLineTextEdit.Text));
        ModelNode.Rotation = new Vector3(float.Parse(_rotationXAxisLineTextEdit.Text), float.Parse(_rotationYAxisLineTextEdit.Text), float.Parse(_rotationZAxisLineTextEdit.Text));
        CameraNode.Size = Math.Max(float.Parse(CamDistancelLineTextEdit.Text), 1.00f);
        CameraNode.RotationDegrees = new Vector3(float.Parse(_camXRotationLineTextEdit.Text), 0, 0);
    }

    private void LoadTransformValueToUI()
    {
        if (_posXAxisLineTextEdit == null || CamDistancelLineTextEdit == null) return;

        _posXAxisLineTextEdit.Text = ModelNode.Position.X.ToString("0.0");
        _posYAxisLineTextEdit.Text = ModelNode.Position.Y.ToString("0.0");
        _posZAxisLineTextEdit.Text = ModelNode.Position.Z.ToString("0.0");

        CamDistancelLineTextEdit.Text = Math.Max(CameraNode.Size, 1.00f).ToString("0.0"); //CameraNode.Size.ToString("0.0");
        _camXRotationLineTextEdit.Text = CameraNode.RotationDegrees.X.ToString("0.0");

        _rotationXAxisLineTextEdit.Text = ModelNode.Rotation.X.ToString("0.0");
        _rotationYAxisLineTextEdit.Text = ModelNode.Rotation.Y.ToString("0.0");
        _rotationZAxisLineTextEdit.Text = ModelNode.Rotation.Z.ToString("0.0");
    }

    public void OnSaveData(SaveGameData newSaveGameData)
    {
        GD.PrintT("Started OnSaveData from:", this.Name);
        newSaveGameData.CameraDistance = float.Parse(CamDistancelLineTextEdit.Text);
        newSaveGameData.CameraRotation = float.Parse(_camXRotationLineTextEdit.Text);
        newSaveGameData.ModelPositionXAxis = float.Parse(_posXAxisLineTextEdit.Text);
        newSaveGameData.ModelPositionYAxis = float.Parse(_posYAxisLineTextEdit.Text);
        newSaveGameData.ModelPositionZAxis = float.Parse(_posZAxisLineTextEdit.Text);
        newSaveGameData.ModelRotationXAxis = float.Parse(_rotationXAxisLineTextEdit.Text);
        newSaveGameData.ModelRotationYAxis = float.Parse(_rotationYAxisLineTextEdit.Text);
        newSaveGameData.ModelRotationZAxis = float.Parse(_rotationZAxisLineTextEdit.Text);



        //nodeSaveData.Add(_spriteResolution);

        // Godot.Collections.Dictionary localNodeData = new();

        // localNodeData["CameraDistance"] = float.Parse(CamDistancelLineTextEdit.Text);
        // //localNodeData[CamDistancelLineTextEdit.Name] = float.Parse(CamDistancelLineTextEdit.Text);
        // localNodeData[nameof(CamDistancelLineTextEdit)] = float.Parse(CamDistancelLineTextEdit.Text);

        // nodeSaveData2[this.Name] = localNodeData;

    }

    public void OnLoadData(SaveGameData newLoadData)
    {
        GD.PrintT("Started OnLoadData from:", this.Name);
        CamDistancelLineTextEdit.Text = newLoadData.CameraDistance.ToString();
        _camXRotationLineTextEdit.Text = newLoadData.CameraRotation.ToString();
        _posXAxisLineTextEdit.Text = newLoadData.ModelPositionXAxis.ToString();
        _posYAxisLineTextEdit.Text = newLoadData.ModelPositionYAxis.ToString();
        _posZAxisLineTextEdit.Text = newLoadData.ModelPositionZAxis.ToString();
        _rotationXAxisLineTextEdit.Text = newLoadData.ModelRotationXAxis.ToString();
        _rotationYAxisLineTextEdit.Text = newLoadData.ModelRotationYAxis.ToString();
        _rotationZAxisLineTextEdit.Text = newLoadData.ModelRotationZAxis.ToString();
        SetTransformValueToModel();
    }



}
