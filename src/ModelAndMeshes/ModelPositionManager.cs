using System;
using Godot;

public partial class ModelPositionManager : Node
{


    // _posXAxisLineTextEdit.Text = ModelNode.Position.X.ToString("0.0");
    //     _posYAxisLineTextEdit.Text = ModelNode.Position.Y.ToString("0.0");
    //     _posZAxisLineTextEdit.Text = ModelNode.Position.Z.ToString("0.0");

    //     CamDistancelLineTextEdit.Text = Math.Max(CameraNode.Size, 1.00f).ToString("0.0"); //CameraNode.Size.ToString("0.0");
    // _camXRotationLineTextEdit.Text = CameraNode.RotationDegrees.X.ToString("0.0");

    //     _rotationXAxisLineTextEdit.Text = ModelNode.Rotation.X.ToString("0.0");
    //     _rotationYAxisLineTextEdit.Text = ModelNode.Rotation.Y.ToString("0.0");
    //     _rotationZAxisLineTextEdit.Text = ModelNode.Rotation.Z.ToString("0.0");

    [Export] public float PositionXValue = 0.0f;
    [Export] public float PositionYValue = -4.0f;
    [Export] public float PositionZValue = 0.0f;
    [Export] public float RotationXValue = 0.0f;
    [Export] public float RotationYValue = 0.0f;
    [Export] public float RotationZValue = 0.0f;

    [Export] public float CameDistance = 7.5f;
    [Export] public float CamRotationXValue = -20.0f;

    [Export] public LineEdit CamXRotationLineTextEdit;

    [Export] public LineEdit CamDistancelLineTextEdit;

    [Export] public LineEdit PosXAxisLineTextEdit;
    [Export] public LineEdit PosYAxisLineTextEdit;
    [Export] public LineEdit PosZAxisLineTextEdit;

    [Export] public LineEdit RotationXAxisLineTextEdit;
    [Export] public LineEdit RotationYAxisLineTextEdit;
    [Export] public LineEdit RotationZAxisLineTextEdit;

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

    public void SetTransformValueToModel(bool firstLoad = false)
    {
        if (firstLoad)
        {
            ModelNode.Position = new Vector3(PositionXValue, PositionYValue, PositionZValue);
            ModelNode.Rotation = new Vector3(RotationXValue, RotationYValue, RotationZValue);
            CameraNode.Size = Math.Max(CameDistance, 1.00f);
            CameraNode.RotationDegrees = new Vector3(CamRotationXValue, 0, 0);
            LoadTransformValueToUI();
        }
        else
        {
            ModelNode.Position = new Vector3(float.Parse(PosXAxisLineTextEdit.Text), float.Parse(PosYAxisLineTextEdit.Text), float.Parse(PosZAxisLineTextEdit.Text));
            ModelNode.Rotation = new Vector3(float.Parse(RotationXAxisLineTextEdit.Text), float.Parse(RotationYAxisLineTextEdit.Text), float.Parse(RotationZAxisLineTextEdit.Text));
            CameraNode.Size = Math.Max(float.Parse(CamDistancelLineTextEdit.Text), 1.00f);
            CameraNode.RotationDegrees = new Vector3(float.Parse(CamXRotationLineTextEdit.Text), 0, 0);
        }

    }


    private void LoadTransformValueToUI()
    {
        if (PosXAxisLineTextEdit == null || CamDistancelLineTextEdit == null) return;

        PosXAxisLineTextEdit.Text = ModelNode.Position.X.ToString("0.0");
        PosYAxisLineTextEdit.Text = ModelNode.Position.Y.ToString("0.0");
        PosZAxisLineTextEdit.Text = ModelNode.Position.Z.ToString("0.0");

        CamDistancelLineTextEdit.Text = Math.Max(CameraNode.Size, 1.00f).ToString("0.0"); //CameraNode.Size.ToString("0.0");
        CamXRotationLineTextEdit.Text = CameraNode.RotationDegrees.X.ToString("0.0");

        RotationXAxisLineTextEdit.Text = ModelNode.Rotation.X.ToString("0.0");
        RotationYAxisLineTextEdit.Text = ModelNode.Rotation.Y.ToString("0.0");
        RotationZAxisLineTextEdit.Text = ModelNode.Rotation.Z.ToString("0.0");
    }

    public void OnSaveData(SaveGameData newSaveGameData)
    {
        GD.PrintT("Started OnSaveData from:", this.Name);
        newSaveGameData.CameraDistance = float.Parse(CamDistancelLineTextEdit.Text);
        newSaveGameData.CameraRotation = float.Parse(CamXRotationLineTextEdit.Text);
        newSaveGameData.ModelPositionXAxis = float.Parse(PosXAxisLineTextEdit.Text);
        newSaveGameData.ModelPositionYAxis = float.Parse(PosYAxisLineTextEdit.Text);
        newSaveGameData.ModelPositionZAxis = float.Parse(PosZAxisLineTextEdit.Text);
        newSaveGameData.ModelRotationXAxis = float.Parse(RotationXAxisLineTextEdit.Text);
        newSaveGameData.ModelRotationYAxis = float.Parse(RotationYAxisLineTextEdit.Text);
        newSaveGameData.ModelRotationZAxis = float.Parse(RotationZAxisLineTextEdit.Text);



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
        CamXRotationLineTextEdit.Text = newLoadData.CameraRotation.ToString();
        PosXAxisLineTextEdit.Text = newLoadData.ModelPositionXAxis.ToString();
        PosYAxisLineTextEdit.Text = newLoadData.ModelPositionYAxis.ToString();
        PosZAxisLineTextEdit.Text = newLoadData.ModelPositionZAxis.ToString();
        RotationXAxisLineTextEdit.Text = newLoadData.ModelRotationXAxis.ToString();
        RotationYAxisLineTextEdit.Text = newLoadData.ModelRotationYAxis.ToString();
        RotationZAxisLineTextEdit.Text = newLoadData.ModelRotationZAxis.ToString();
        SetTransformValueToModel();
    }



}
