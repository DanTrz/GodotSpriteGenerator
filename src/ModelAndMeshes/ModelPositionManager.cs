using System;
using Godot;

public partial class ModelPositionManager : Node
{
    [Export] public Vector3 ModelPosition = new Vector3(0.0f, -0.0f, 0.0f);

    [Export] public Vector3 ModelRotation = new Vector3(0.0f, -0.0f, 0.0f);

    [Export]
    public float CameDistance = 80.0f;

    [Export] public float ZoomValue = 0.5f;
    [Export] public float ZoomScaleFactor = 1.8f;
    [Export] public float YPosScaleFactor = -1.6f;
    [Export] public float CamRotationXValue = -20.0f;

    [Export] public LineEdit CamXRotationLineTextEdit;

    [Export] public LineEdit CamDistancelLineTextEdit;

    [Export] public LineEdit PosXAxisLineTextEdit;
    [Export] public LineEdit PosYAxisLineTextEdit;
    [Export] public LineEdit PosZAxisLineTextEdit;

    [Export] public LineEdit RotationXAxisLineTextEdit;
    [Export] public LineEdit RotationYAxisLineTextEdit;
    [Export] public LineEdit RotationZAxisLineTextEdit;

    private Node3D _modelPivotNode;
    private Camera3D _cameraNode;
    private bool _isModeLeftBtnHeld = false;

    public override void _Ready()
    {
        GlobalEvents.Instance.OnModelTransformChanged += OnModelTransformChanged;
    }

    public void SetDependencies(Node3D modelPivotNode, Camera3D cameraNode)
    {
        _modelPivotNode = modelPivotNode;
        _cameraNode = cameraNode;

        //Check if the ModelNode or Camera are not null
        if (!CheckIfModelLoaded()) return;

        //("LoadTransformValueToUI being called");
        this.CallDeferred(MethodName.LoadTransformValueToUI);
        this.CallDeferred(MethodName.ConnectTransformUINodeSignals);
    }

    public bool CheckIfModelLoaded()
    {
        if (_modelPivotNode == null || _cameraNode == null)
        {
            Log.Error("Model or Camera in ModelPositionManager is null");
            return false;
        }
        return true;
    }


    private void ConnectTransformUINodeSignals()
    {
        foreach (var node in GlobalUtil.GetAllChildNodesByType<LineEdit>(this))
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
            //Log.Debug("Valid Input: " + newValue);
            SetTransformValueToModel();
            //LoadTransformValueToUI();
        }
    }

    public void SetTransformValueToModel(bool autoScale = false, float modelXAxisSize = 0.0f)
    {
        if (autoScale)
        {
            ModelPosition.Y = GetYPositionAutoScaleValue(modelXAxisSize, YPosScaleFactor);
            CameDistance = GetCameraAutoScaleValue(modelXAxisSize, ZoomScaleFactor);

            _modelPivotNode.Position = new Vector3(ModelPosition.X, ModelPosition.Y, ModelPosition.Z); //new Vector3(PositionXValue, PositionYValue, PositionZValue);
            _modelPivotNode.Rotation = new Vector3(ModelRotation.X, ModelRotation.Y, ModelRotation.Z); //new Vector3(RotationXValue, RotationYValue, RotationZValue);
            _cameraNode.Size = Math.Max(CameDistance, 1.00f);
            _cameraNode.RotationDegrees = new Vector3(CamRotationXValue, 0, 0);
            LoadTransformValueToUI();
        }
        else
        {
            _modelPivotNode.Position = new Vector3(float.Parse(PosXAxisLineTextEdit.Text), float.Parse(PosYAxisLineTextEdit.Text), float.Parse(PosZAxisLineTextEdit.Text));
            _modelPivotNode.Rotation = new Vector3(float.Parse(RotationXAxisLineTextEdit.Text), float.Parse(RotationYAxisLineTextEdit.Text), float.Parse(RotationZAxisLineTextEdit.Text));
            _cameraNode.Size = Math.Max(float.Parse(CamDistancelLineTextEdit.Text), 1.00f);
            _cameraNode.RotationDegrees = new Vector3(float.Parse(CamXRotationLineTextEdit.Text), 0, 0);
        }

    }

    private float GetYPositionAutoScaleValue(float modelXAxisSize, float scaleValue)
    {
        float yPos = 0.0f;
        if (modelXAxisSize > 0)
        {
            yPos = modelXAxisSize / scaleValue;
            double roundedValue = Math.Floor(yPos * 1) / 1; //Ensures we round-down with 1 decimal place
            return (float)roundedValue;

        }
        return 0.0f;
    }

    private float GetCameraAutoScaleValue(float modelXAxisSize, float scaleValue)
    {
        float camZoomValue = 0.0f;
        if (modelXAxisSize > 0)
        {
            // camZoomValue = float.Round((modelXAxisSize * scaleValue));
            camZoomValue = modelXAxisSize * scaleValue;
            double roundedValue = Math.Ceiling(camZoomValue * 1) / 1; //Ensures we round-up with with 1 decimal place
            return (float)roundedValue;
        }
        return 0.0f;
    }

    private void LoadTransformValueToUI()
    {
        if (PosXAxisLineTextEdit == null || CamDistancelLineTextEdit == null) return;

        PosXAxisLineTextEdit.Text = _modelPivotNode.Position.X.ToString("0.0");
        PosYAxisLineTextEdit.Text = _modelPivotNode.Position.Y.ToString("0.0");
        PosZAxisLineTextEdit.Text = _modelPivotNode.Position.Z.ToString("0.0");

        CamDistancelLineTextEdit.Text = Math.Max(_cameraNode.Size, 1.00f).ToString("0.0"); //CameraNode.Size.ToString("0.0");
        CamXRotationLineTextEdit.Text = _cameraNode.RotationDegrees.X.ToString("0.0");

        RotationXAxisLineTextEdit.Text = _modelPivotNode.Rotation.X.ToString("0.0");
        RotationYAxisLineTextEdit.Text = _modelPivotNode.Rotation.Y.ToString("0.0");
        RotationZAxisLineTextEdit.Text = _modelPivotNode.Rotation.Z.ToString("0.0");
    }

    public void OnSaveData(SaveGameData newSaveGameData)
    {
        //Log.Debug("Started OnSaveData from:", this.Name);
        newSaveGameData.CameraDistance = float.Parse(CamDistancelLineTextEdit.Text);
        newSaveGameData.CameraRotation = float.Parse(CamXRotationLineTextEdit.Text);
        newSaveGameData.ModelPositionXAxis = float.Parse(PosXAxisLineTextEdit.Text);
        newSaveGameData.ModelPositionYAxis = float.Parse(PosYAxisLineTextEdit.Text);
        newSaveGameData.ModelPositionZAxis = float.Parse(PosZAxisLineTextEdit.Text);
        newSaveGameData.ModelRotationXAxis = float.Parse(RotationXAxisLineTextEdit.Text);
        newSaveGameData.ModelRotationYAxis = float.Parse(RotationYAxisLineTextEdit.Text);
        newSaveGameData.ModelRotationZAxis = float.Parse(RotationZAxisLineTextEdit.Text);

    }

    public void OnLoadData(SaveGameData newLoadData)
    {
        // Log.Debug("Started OnLoadData from:", this.Name);
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

    private void OnModelTransformChanged(int mode, Vector3 vector)
    {
        //Log.Debug("OnModelPivot Gizmo TransformChanged");
        LoadTransformValueToUI();
    }

    private void OnCameraZoomChanged(bool zoomIn)
    {
        if (zoomIn)
        {
            CameDistance -= ZoomValue;
            _cameraNode.Size = Math.Max(CameDistance, 1.00f);

        }
        else
        {
            CameDistance += ZoomValue;
            _cameraNode.Size = Math.Max(CameDistance, 1.00f);
        }

        LoadTransformValueToUI();

    }
}
