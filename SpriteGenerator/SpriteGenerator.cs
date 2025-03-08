using System;
using System.IO;
using System.Linq;
using Godot;

public partial class SpriteGenerator : Node
{
    [Export] public Button _startGenerationBtn;

    [Export] public Node3D MainScene3D;
    [Export] public SubViewport _rawViewport;
    [Export] public SubViewportContainer _rawViewportContainer;

    //private string _saveSpriteFolderPath;
    //public string SaveSpriteFolderPath;
    ////public string SaveSpriteFolderPath
    ////{
    ////    get
    ////    {
    ////        return _saveSpriteFolderPath;
    ////    }
    ////    set
    ////    {
    ////        _saveSpriteFolderPath = value;
    ////        _spriteGenFolderPathLineEdit.Text = _saveSpriteFolderPath;
    ////    }
    ////}

    public static int _spriteResolution = 256;
    [Export] public int frameSkipStep = 4; // Control how frequently frames are captured
    [Export] public bool _clearFolderBeforeGeneration = true;
    [Export] public bool _usePixelEffect = true;

    [Export(PropertyHint.Range, "1,4,1")] private float _animationPlaybackSpeed = 1.0f;


    //SaveConfigBtn

    [Export] public Button _saveConfigBtn;
    [Export] public OptionButton SavedConfigListOptBtn;
    [Export] public Button _loadConfigBtn;

    [Export] public OptionButton _resolutionOptionBtn;
    [Export] public OptionButton _pixelShaderOptionBtn;
    [Export] public LineEdit _frameStepTextEdit;
    [Export] public LineEdit _playBackSpeedLineEdit;
    [Export] public CheckButton _clearFolderCheckBtn;
    [Export] public CheckButton _pixelEffectCheckBtn;
    [Export] public TextureRect _pixelShaderTextRect;
    [Export] public ModelPositionManager _modelPositionManager;
    [Export] public Button _loadAllAnimationsBtn;
    [Export] public ItemListCheckBox _animSelectionItemList;
    [Export] public ItemListCheckBox _angleSelectionItemList;
    [Export] public TextureRect _pixelGridTextRect;
    [Export] public CheckButton _showGridCheckButton;

    //Main Settings and Folder Path Variables
    [Export] public Button _selectFolderPathBtn;
    [Export] public Button _openFolderPathBtn;
    [Export] public LineEdit _spriteGenFolderPathLineEdit;
    [Export] public MarginContainer _settingsMainPanel;
    [Export] public Button _openSettingPanelBtn;
    //[OnReady("%MainTabContainer")] TabContainer _mainTabContainer;


    //SettingsMarginOptionsPanel


    //MeshReplacer Nodes and Variables
    [Export] public PanelContainer _meshReplacerPanelParentNode;
    [Export] public OptionButton _hairMeshOptBtn;
    [Export] public ColorPickerButton _hairColorBtn;
    //[OnReady("%HeadMeshOptBtn")] private OptionButton _headMeshOptBtn;
    //[OnReady("%TorsoMeshOptBtn")] private OptionButton _torsoMeshOptBtn;


    //[OnReady("%LegsMeshOptBtn")] private OptionButton _legsMeshOptBtn;
    //[OnReady("%FeetMeshOptBtn")] private OptionButton _feetMeshOptBtn;
    //[OnReady("%RightArmMeshOptBtn")] private OptionButton _rightArmMeshOptBtn;
    //[OnReady("%LeftArmMeshOptBtn")] private OptionButton _leftArmMeshOptBtn;
    //[OnReady("%TorsoItemMeshOptBtn")] private MeshReplacerOptButton _torsoItemMeshOptBtn;



    private Node3D _modelPivotNode;
    private Node3D _characterModelObject;
    private Camera3D _camera;
    private AnimationPlayer _animationPlayer;

    private readonly int[] allAngles = { 0, 45, 90, 135, 180, 225, 270, 315 };
    private int renderAngle = 0;
    private string currentAnimation;
    private string currentAnimationName;
    private int frameIndex;
    private int spriteCount = 1;
    private string saveFolder = "Model";

    private Control _lastFocusedControl;

    public override void _Ready()
    {

        //Connect UI Signals
        _saveConfigBtn.Pressed += OnSaveConfigBtnPressed;
        _loadConfigBtn.Pressed += OnLoadConfigBtnPressed;
        _spriteGenFolderPathLineEdit.TextChanged += (newDir) => GlobalUtil.OnFolderSelected(newDir, _spriteGenFolderPathLineEdit);
        _selectFolderPathBtn.Pressed += OnSelectFolderPathPressed;
        _openFolderPathBtn.Pressed += OnOpenFolderPathPressed;
        _startGenerationBtn.Pressed += OnStartGeneration;
        _resolutionOptionBtn.ItemSelected += OnRenderResolutionChanged;
        _pixelShaderOptionBtn.ItemSelected += OnPixelShaderResolutionChanged;
        _frameStepTextEdit.TextChanged += OnFrameStepChanged;
        _playBackSpeedLineEdit.TextChanged += OnPlayBackSpeedChanged;
        _clearFolderCheckBtn.Pressed += OnClearFolderPressed;
        _pixelEffectCheckBtn.Pressed += OnPixelEffectPressed;
        _loadAllAnimationsBtn.Pressed += OnLoadAllAnimationsPressed;
        //_saveIntervalTimer.Timeout += OnSaveIntervalTimerTimeout;
        _showGridCheckButton.Pressed += OnShowGridCheckButtonPressed;
        _openSettingPanelBtn.Pressed += () => _settingsMainPanel.Visible = !_settingsMainPanel.Visible;
        //_mainTabContainer.MouseEntered += () => GD.Print("Tab Cointainer Mouse Entered"); //_settingsMainPanel.Visible = false;

        //MeshReplacer Signals
        _hairColorBtn.ColorChanged += OnHairColorChanged;
        _hairMeshOptBtn.ItemSelected += OnHairMeshOptBtnItemSelected;

        //Set Default UI Control Values
        _settingsMainPanel.Visible = false;
        _spriteGenFolderPathLineEdit.Text = GlobalUtil.SaveFolderPath;
        _clearFolderCheckBtn.ButtonPressed = _clearFolderBeforeGeneration;
        _pixelEffectCheckBtn.ButtonPressed = _usePixelEffect;
        _pixelShaderTextRect.Visible = _usePixelEffect;
        _frameStepTextEdit.Text = frameSkipStep.ToString();
        _playBackSpeedLineEdit.Text = _animationPlaybackSpeed.ToString();
        _angleSelectionItemList.CreateItemsFromList(allAngles.Select(x => x.ToString()).ToArray());

        //Effects and Core Grid Options
        _showGridCheckButton.ButtonPressed = true;
        _showGridCheckButton.Text = _showGridCheckButton.ButtonPressed.ToString();
        _pixelEffectCheckBtn.Text = _pixelEffectCheckBtn.ButtonPressed.ToString();

        //Set Default Resolution and Shader Strenght
        _resolutionOptionBtn.Selected = _resolutionOptionBtn.ItemCount - 1;
        OnRenderResolutionChanged(_resolutionOptionBtn.ItemCount - 1);
        _pixelShaderOptionBtn.Selected = _pixelShaderOptionBtn.ItemCount - 1;
        OnPixelShaderResolutionChanged(_pixelShaderOptionBtn.ItemCount - 1);



        //Pass the objects from MainScene3D to the SpriteGenerator
        if (MainScene3D != null)
        {
            //_modelPivotNode = MainScene3D.MainModelNode;
            //_camera = MainScene3D.MainCamera;
            //_animationPlayer = MainScene3D.MainAnimationPlayer;
            //_characterModelObject = MainScene3D.MainCharacterObj;

            //Get Reference to Our Object3D within MainScene
            _modelPivotNode = MainScene3D.GetNode<Node3D>("%Model3DMainPivotControl");
            _camera = MainScene3D.GetNode<Camera3D>("%MainCamera");
            _characterModelObject = _modelPivotNode.GetChild<Node3D>(0);
            _animationPlayer = _characterModelObject.GetNode<AnimationPlayer>("AnimationPlayer");

            //Pass the Model to te PositionManager 
            _modelPositionManager.ModelNode = _modelPivotNode;
            _modelPositionManager.CameraNode = _camera;
        }
        else
        {
            GD.PrintErr("MainScene3D is null");
        }

        //_animationPlayer.AnimationFinished += OnAnimationFinished;


        //Mesh Replace Variables and UI
        MeshReplacer.UpdateUIOptionsHairList(_hairMeshOptBtn);
        LoadAllMeshReplacerBtnAndMeshItemData();
        _hairMeshOptBtn.Selected = 0;
        OnHairMeshOptBtnItemSelected(0);
        _hairColorBtn.Color = Colors.White;


        //UpdateAllMeshesAndMeshesUI();
        UpdateViewPort();

    }

    private async void OnLoadConfigBtnPressed()
    {
        using Godot.FileDialog fileDialog = new Godot.FileDialog
        {
            FileMode = FileDialog.FileModeEnum.OpenFile,
            Filters = new string[] { "*.tres ; SaveGame File" },
            Access = FileDialog.AccessEnum.Filesystem
        };

        AddChild(fileDialog);

        fileDialog.CurrentDir = ProjectSettings.GlobalizePath(Const.SAVE_GAME_PATH); //Set this after adding Child to Scene

        //fileDialog.DirSelected += (newDir) => GlobalUtil.OnFolderSelected(newDir, _spriteGenFolderPathLineEdit);

        //fileDialog.FileSelected += async (path) => await SaveGameManager.Instance.LoadGameData(path);

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        fileDialog.PopupCentered();

        await ToSignal(fileDialog, FileDialog.SignalName.FileSelected);

        string file = fileDialog.CurrentFile;
        string fullLoadFilePath = fileDialog.CurrentDir + "/" + file;

        await SaveGameManager.Instance.LoadGameData(fullLoadFilePath);

    }


    private async void OnSaveConfigBtnPressed()
    {
        using FileDialog fileDialog = new FileDialog
        {
            FileMode = FileDialog.FileModeEnum.SaveFile,
            Filters = new string[] { "*.tres ; SaveGame File" },
            Access = FileDialog.AccessEnum.Filesystem
        };

        AddChild(fileDialog); // Add to scene first 

        //string folderCurrentDir = Const.USER_ROOT_FOLDER_PATH; // Ensure it's inside user:// or res://
        string globalizedSavePath = ProjectSettings.GlobalizePath(Const.SAVE_GAME_PATH);

        if (!GlobalUtil.HasDirectory(globalizedSavePath, this))
        {
            GD.Print("Directory does NOT exist: " + globalizedSavePath);
            globalizedSavePath = "user://"; // Fallback to a safe default
        }

        fileDialog.CurrentDir = globalizedSavePath; //Set Current Directory at the end after adding Child to Scene otherwise it was not working

        fileDialog.FileSelected += async (path) => await SaveGameManager.Instance.SaveGameData(path);

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        fileDialog.PopupCentered(); // Show the dialog

        //await SaveGameManager.Instance.SaveGameData(saveFileName);
    }

    private void OnStartGeneration()
    {
        if (!GlobalUtil.HasDirectory(GlobalUtil.SaveFolderPath, this)) return;

        spriteCount = 0;
        saveFolder = ProjectSettings.GlobalizePath(GlobalUtil.SaveFolderPath + "/" + _characterModelObject.Name);

        _pixelGridTextRect.Visible = false;

        if (!Directory.Exists(ProjectSettings.GlobalizePath(saveFolder)))
            Directory.CreateDirectory(ProjectSettings.GlobalizePath(saveFolder));

        // GD.PrintT("Start Generation");

        if (_clearFolderBeforeGeneration)
            ClearFolder(saveFolder);


        GenerateSpritesFrameBased();


    }

    private void ClearFolder(string folder)
    {
        string[] files = Directory.GetFiles(folder);
        foreach (string file in files)
        {
            File.Delete(file);
        }
    }

    private async void GenerateSpritesFrameBased()
    {

        int[] selectedAngles = _angleSelectionItemList.GetSelectedItems().Select(x => Convert.ToInt32(_angleSelectionItemList.
        GetItemText(x))).ToArray();
        int[] selectedAnimations = _animSelectionItemList.GetSelectedItems();

        if (selectedAngles.Length == 0)
        {
            GD.PrintErr("No Angles Selected");
            return;
        }

        if (selectedAnimations.Length == 0)
        {
            GD.PrintErr("No Aniimations Selected");
            return;
        }

        _animationPlayer.Stop();
        _animationPlayer.SpeedScale = _animationPlaybackSpeed;
        //Engine.TimeScale = _animationPlaybackSpeed;

        _modelPivotNode.RotationDegrees = new Vector3(0, 0, 0);

        foreach (var selectedAnimItem in _animSelectionItemList.GetSelectedItems())
        {
            string anim = _animSelectionItemList.GetItemText(selectedAnimItem);

            if (anim == "RESET" || anim == "TPose") continue;

            currentAnimation = anim;
            currentAnimationName = anim.Replace("/", "_");

            foreach (var angle in selectedAngles)
            {
                _modelPivotNode.RotationDegrees = new Vector3(0, angle, 0);

                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

                _animationPlayer.Play(anim);
                Animation animationResource = _animationPlayer.GetAnimation(anim);

                float frameCount = (int)Math.Round((animationResource.Length / animationResource.Step), MidpointRounding.AwayFromZero); // Number of frames in the animation
                int framesToRender = (int)Math.Round((frameCount / frameSkipStep), MidpointRounding.AwayFromZero);

                //(int)Math.Round(value, MidpointRounding.AwayFromZero);

                GD.PrintT($"FrameBASEDv2 FLOAT = animFrames: {(animationResource.Length / animationResource.Step):0.00}, will render: {(frameCount / frameSkipStep):0.00} frames");

                GD.PrintT($"FrameBASEDv2 INT = animFrames: {frameCount:0.00}, will render: {framesToRender:0.00} frames");

                //(int)Math.Ceiling(value);

                //float frameInterval = animationResource.Step * _animationPlaybackSpeed;
                float frameInterval = animationResource.Step;

                GD.PrintT($"FrameBASEDv2 = frameInterval {frameInterval:0.000}");
                float currentTime = 0f;
                int currentFrame = 0;

                // while (currentTime < animationResource.Length)
                while (_animationPlayer.IsPlaying())
                {

                    await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                    //await ToSignal(GetTree().CreateTimer(0.0001f), Timer.SignalName.Timeout);

                    _animationPlayer.Seek(currentTime, true); // Seek to the exact time

                    //GD.PrintT("AnimSeek to: " + currentTime);

                    //Only capture frames where frameIndex is a multiple of frameStep
                    if (currentFrame % frameSkipStep == 0)
                    {
                        SaveFrameAsImgPNG(angle);
                    }

                    currentTime += frameInterval;

                    currentFrame++;
                }
            }

            _modelPivotNode.RotationDegrees = new Vector3(0, 0, 0);
        }

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        GenerateSpriteSheet(saveFolder, currentAnimationName + "_spriteSheet", 4);

    }

    private void SaveFrameAsImgPNG(int angle)
    {
        string currentAnimPosInSec = ((float)_animationPlayer.CurrentAnimationPosition).ToString("0.000");

        GD.PrintT("SavingFile : ", spriteCount + " / AnimSecond: " + currentAnimPosInSec);

        //GD.PrintT("Frame: " + frameIndex, " AnimPosition: " + (float)_animationPlayer.CurrentAnimationPosition);
        var img = _rawViewport.GetTexture().GetImage();

        //img.FlipY();4
        string path = $"{saveFolder}/{currentAnimationName}_{"angle_" + angle}_{spriteCount}.png";
        spriteCount++;

        img.SavePng(ProjectSettings.GlobalizePath(path));
        frameIndex++;

    }


    private void GenerateSpriteSheet(string folderPath, string outputFileName, int columns)
    {
        var dirPath = ProjectSettings.GlobalizePath(folderPath);
        if (!Directory.Exists(dirPath))
        {
            GD.PrintErr("Directory does not exist: " + dirPath);
            return;
        }

        var imagePaths = Directory.GetFiles(dirPath, "*.png");
        if (imagePaths.Length == 0)
        {
            GD.PrintErr("No PNG images found in: " + dirPath);
            return;
        }

        // Load first image to get dimensions
        var firstImage = Image.LoadFromFile(imagePaths[0]);
        if (firstImage == null)
        {
            GD.PrintErr("Failed to load image: " + imagePaths[0]);
            return;
        }

        int spriteWidth = firstImage.GetWidth();
        int spriteHeight = firstImage.GetHeight();

        int rows = (int)Math.Ceiling((double)imagePaths.Length / columns);
        int sheetWidth = spriteWidth * columns;
        int sheetHeight = spriteHeight * rows;

        // Create a new blank image for the sprite sheet
        //var spriteSheet = Image.Create(sheetWidth, sheetHeight, false, Image.Format.Rgba8);
        var spriteSheet = Image.CreateEmpty(sheetWidth, sheetHeight, false, Image.Format.Rgba8);

        for (int i = 0; i < imagePaths.Length; i++)
        {
            var img = Image.LoadFromFile(imagePaths[i]);
            if (img == null) continue;

            int x = (i % columns) * spriteWidth;
            int y = (i / columns) * spriteHeight;

            spriteSheet.BlitRect(img, new Rect2I(0, 0, spriteWidth, spriteHeight), new Vector2I(x, y));
        }

        // Save the final sprite sheet
        string outputPath = ProjectSettings.GlobalizePath(folderPath + "/" + outputFileName + ".png");
        spriteSheet.SavePng(outputPath);

        GD.Print("Sprite sheet saved: " + outputPath);

        _pixelGridTextRect.Visible = true;
    }
    private void OnOpenFolderPathPressed()
    {

        string directory = ProjectSettings.GlobalizePath(_spriteGenFolderPathLineEdit.Text);
        if (GlobalUtil.HasDirectory(directory, this))
        {
            OS.ShellOpen(directory);
        }
        else
        {
            GD.PrintErr("Directory does not exist: " + directory);

            using Godot.AcceptDialog acceptDialog = new Godot.AcceptDialog
            {
                Title = "Error: Directory not Found",
                DialogText = "Directory does not exist: " + directory
            };

            AddChild(acceptDialog);
            acceptDialog.PopupCentered();
        }
    }

    private void OnSelectFolderPathPressed()
    {
        using Godot.FileDialog fileDialog = new Godot.FileDialog
        {
            FileMode = FileDialog.FileModeEnum.OpenDir,
            Access = FileDialog.AccessEnum.Filesystem
        };

        AddChild(fileDialog);

        fileDialog.CurrentDir = GlobalUtil.SaveFolderPath; //Set this after adding Child to Scene

        fileDialog.DirSelected += (newDir) => GlobalUtil.OnFolderSelected(newDir, _spriteGenFolderPathLineEdit);

        fileDialog.PopupCentered();

    }

    private void OnRenderResolutionChanged(long itemSelectedIndex)
    {
        switch (itemSelectedIndex)
        {
            case 0:
                _spriteResolution = 64;
                break;
            case 1:
                _spriteResolution = 128;
                break;
            case 2:
                _spriteResolution = 256;
                break;
            case 3:
                _spriteResolution = 512;
                break;
        }
        GD.PrintT("Selected resolution: " + _spriteResolution);

        UpdateViewPort();
    }


    private void OnPixelShaderResolutionChanged(long itemSelectedIndex)
    {
        int shaderResolution = 0;

        switch (itemSelectedIndex)
        {
            case 0:
                shaderResolution = 32;
                break;
            case 1:
                shaderResolution = 64;
                break;
            case 2:
                shaderResolution = 96;
                break;
            case 3:
                shaderResolution = 108;
                break;
            case 4:
                shaderResolution = 168;
                break;
            case 5:
                shaderResolution = 218;
                break;
        }

        //((ShaderMaterial)_pixelShaderTextRect.MaterialOverride).SetShaderParameter("pixel_size", shaderResolution);
        //GD.PrintT("Shader resolution: " + shaderResolution);

        //_pixelShaderTextRect.Material.


        if (_pixelShaderTextRect.Material is ShaderMaterial shaderMaterial)
        {
            shaderMaterial.SetShaderParameter("pixel_size", shaderResolution);
        }






        UpdateViewPort();
    }

    private void OnPlayBackSpeedChanged(string newText)
    {
        if (string.IsNullOrEmpty(newText)) return;

        _animationPlaybackSpeed = Convert.ToInt32(newText);
        GD.PrintT("PlaybackSpeed: " + _animationPlaybackSpeed);

    }

    private void UpdateViewPort()
    {
        Vector2I viewPortSize = new Vector2I(_spriteResolution, _spriteResolution);
        _rawViewport.CallDeferred("set_size", viewPortSize);
        _rawViewportContainer.CallDeferred("set_size", viewPortSize);

        //_viewport.Size = viewPortSize;
        //_viewportContainer.Size = viewPortSize;

    }
    private void OnClearFolderPressed()
    {
        _clearFolderBeforeGeneration = _clearFolderCheckBtn.ButtonPressed;
        GD.PrintT("Clear Folder: " + _clearFolderBeforeGeneration);
    }


    private void OnFrameStepChanged(string newText)
    {
        if (string.IsNullOrEmpty(newText)) return;

        frameSkipStep = Convert.ToInt32(newText);
        GD.PrintT("Frame Step: " + frameSkipStep);

    }

    private void OnShowGridCheckButtonPressed()
    {
        _pixelGridTextRect.Visible = _showGridCheckButton.ButtonPressed;
        _showGridCheckButton.Text = _showGridCheckButton.ButtonPressed.ToString();

    }


    private void OnPixelEffectPressed()
    {
        _usePixelEffect = _pixelEffectCheckBtn.ButtonPressed;
        _pixelShaderTextRect.Visible = _usePixelEffect;

        _pixelEffectCheckBtn.Text = _pixelEffectCheckBtn.ButtonPressed.ToString();

        GD.PrintT("Use PixelEffect: " + _usePixelEffect);



        // if (_usePixelEffect)
        // {
        //     _viewport.CallDeferred("set_use_pixel_effect", true);
        // }
        // else
        // {
        //     _viewport.CallDeferred("set_use_pixel_effect", false);
    }

    private void OnLoadAllAnimationsPressed()
    {
        //_itemListWithCheckBox

        foreach (var animationItem in _animationPlayer.GetAnimationList())
        {
            if (animationItem == "RESET" || animationItem == "TPose") continue;
            _animSelectionItemList.AddItem(animationItem, _animSelectionItemList.ICON_UNSELECTED, true);

        }
    }

    private void OnHairColorChanged(Color newColor)
    {
        MeshInstance3D _hairMeshObject = _characterModelObject.GetNode<BoneAttachment3D>("%HairBoneAttach").GetChild(0).GetNode<MeshInstance3D>("%HairMesh");


        if (_hairMeshObject != null && _hairMeshObject.GetActiveMaterial(0) is StandardMaterial3D material)
        {
            material.AlbedoColor = newColor;
        }



    }

    private void OnHairMeshOptBtnItemSelected(long index)
    {
        BoneAttachment3D _hairBoneAttachNode = _characterModelObject.GetNode<BoneAttachment3D>("%HairBoneAttach");
        string itemSelected = _hairMeshOptBtn.GetItemText((int)index);
        MeshReplacer.UpdateHairScene(_hairBoneAttachNode, Const.HAIR_SCENES_FOLDER_PATH + itemSelected + ".tscn");
    }


    private void LoadAllMeshReplacerBtnAndMeshItemData()
    {
        var allMeshReplacerOptButtons = GlobalUtil.GetAllNodesByType<MeshReplacerOptButton>(_meshReplacerPanelParentNode);
        GD.PrintT("Found " + allMeshReplacerOptButtons.Count + " MeshReplacerOptButton");

        foreach (var meshReplacerOptButton in allMeshReplacerOptButtons)
        {
            //Connecct the signal and load items
            meshReplacerOptButton.ItemSelected += (itemIndex) => OnMeshItemSelected(itemIndex, meshReplacerOptButton);
            MeshReplacer.UpdateUIOptionMesheItemList(meshReplacerOptButton, meshReplacerOptButton.BodyPartType);

            //Align UI display to the first item
            meshReplacerOptButton.Selected = 0;
            OnMeshItemSelected(0, meshReplacerOptButton);
        }

    }

    //Change the Mesh base based on the MeshReplacaeOpt button selection (Connected signal)
    private void OnMeshItemSelected(long itemIndex, MeshReplacerOptButton meshReplacerOptButton)
    {

        string itemSelected = meshReplacerOptButton.GetItemText((int)itemIndex);
        GD.PrintT("Mesh Item Selected: " + itemSelected + " + FromButton: " + meshReplacerOptButton.Name);


        //Gets the only Skeletion we have in the Model Scene
        Skeleton3D _parentSkeletion = GlobalUtil.GetAllNodesByType<Skeleton3D>(_characterModelObject).FirstOrDefault();

        var _meshInstanceObject = GlobalUtil.GetAllNodesByType<BodyPartMeshInstance3D>(_characterModelObject).
            Where(x => x.BodyPartType == meshReplacerOptButton.BodyPartType).FirstOrDefault();

        MeshReplacer.UpdateMeshFromResourceItem(_meshInstanceObject, itemSelected);
    }


    public void OnSaveData(SaveGameData newSaveGameData)
    {
        GD.PrintT("Started OnSaveData from:", this.Name);
        //nodeSaveData.Add(_spriteResolution);
        newSaveGameData.SpriteResolution = _spriteResolution;
        newSaveGameData.SpriteResolutionOptBtn = _resolutionOptionBtn.Selected;
        newSaveGameData.FrameSkipStep = frameSkipStep;
        newSaveGameData.ShowPixelEffect = _pixelEffectCheckBtn.ButtonPressed;
        newSaveGameData.PixelEffectLevel = _pixelShaderOptionBtn.Selected;
        newSaveGameData.PlaybackSpeed = _animationPlaybackSpeed;
        newSaveGameData.ShowGrid = _showGridCheckButton.ButtonPressed;
    }

    public void OnLoadData(SaveGameData newLoadData)
    {
        GD.PrintT("Started OnLoadData from:", this.Name);
        _spriteResolution = newLoadData.SpriteResolution;
        _resolutionOptionBtn.Selected = newLoadData.SpriteResolutionOptBtn;
        frameSkipStep = newLoadData.FrameSkipStep;
        _frameStepTextEdit.Text = frameSkipStep.ToString();
        _pixelEffectCheckBtn.ButtonPressed = newLoadData.ShowPixelEffect;
        _pixelEffectCheckBtn.Text = _pixelEffectCheckBtn.ButtonPressed.ToString();
        _pixelShaderOptionBtn.Selected = newLoadData.PixelEffectLevel;
        _animationPlaybackSpeed = newLoadData.PlaybackSpeed;
        _playBackSpeedLineEdit.Text = _animationPlaybackSpeed.ToString();
        _showGridCheckButton.ButtonPressed = newLoadData.ShowGrid;

        UpdateViewPort();
        OnRenderResolutionChanged(newLoadData.SpriteResolutionOptBtn);
        OnPixelShaderResolutionChanged(newLoadData.PixelEffectLevel);
        OnPlayBackSpeedChanged(newLoadData.PlaybackSpeed.ToString());
        OnFrameStepChanged(newLoadData.FrameSkipStep.ToString());
        OnShowGridCheckButtonPressed();
        OnPixelEffectPressed();

        _settingsMainPanel.Visible = false;
    }








}
