
using Godot;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public partial class SpriteGenerator : Node
{
    [Export] public OptionButton AnimMethodOptionBtn;
    [Export] public CheckButton GenerateSpriteSheetCheckBtn;
    [Export] public LineEdit SpriteSheetNameTextEdit;
    [Export] public CheckButton ShowMeshPanelCheckBtn;
    [Export] public MarginContainer MeshOptionsMarginCont;
    // [Export] public Button _startGenerationBtn;
    [Export] public Button StartGenerationBtn;

    [Export] public PanelContainer MainRenderPanel;
    [Export] public ModelScene3d MainModelScene;
    [Export] public SubViewport DisplaySubViewPort;
    [Export] public SubViewport RawViewport;
    [Export] public SubViewportContainer _rawViewportContainer;
    [Export] public SubViewport BgRemoverViewport;
    [Export] public SubViewport ImgColorReductionSubViewport;
    [Export] public SubViewportContainer ImgColorReductionSubViewportContainer;
    // [Export] public SubViewport PixelSmoothEffectSubViewport;
    // [Export] public SubViewportContainer PixelSmoothEffectViewPortContainer;
    [Export] public Sprite2D PixelSmoothEffectSprite2D;

    [Export] public TextureRect ImgColorReductionTextRect;
    [Export] public SubViewportContainer BgRemoverViewportContainer;
    //[Export] public MeshInstance3D MeshShaderPixel3D;

    [Export] public TextureRect PixelShaderTextRect;
    [Export] public BoolCheckButton MoveSpriteSheetCheckButton;
    [Export] public int FrameSkipStep = 4; // Control how frequently frames are captured
    [Export] public bool ClearFolderBeforeGeneration = true;
    [Export(PropertyHint.Range, "1,4,1")] private float _animationPlaybackSpeed = 1.0f;
    //[Export] public OptionButton? SpriteSizeOptionButton;
    public OptionButton SpriteSizeOptionButton => field ??= GetNodeOrNull<OptionButton>("%SpriteSizeOptionButton");
    [Export] public OptionButton EffectsChoicesOptionBtn;
    [Export] public OptionButton PixelationLevelOptionBtn;
    [Export] public SliderValueBox Outline3DValueSlider;
    [Export] public SliderValueBox Outline3DBlendFactorSlider;
    public MeshInstance3D Outline3DShaderMesh;
    [Export] public ColorPickerButton Outline3DColorPicker;

    MeshInstance3D Depthline3DShaderMesh;
    [Export] public ColorPickerButton Depthline3DColorPicker;
    [Export] public SliderValueBox DepthlineThicknessSlider;
    [Export] public SliderValueBox DepthBlendValueSlider;
    [Export] public SliderValueBox DepthThresholdSlider;

    [Export] public HSlider DitheringStrenghtSlider;
    [Export] public LineEdit FrameStepTextEdit;


    [Export] public OptionButton ModelTypeOptionButton;
    [Export] public Button LoadExternalModelBtn;

    [Export] public BoolCheckButton AutoScaleModelCheckButton;
    [Export] public CheckButton EnableHairMeshCheckBtn;

    [Export] public SpinBox MaxColorPaletteSpinBox;

    [Export] public LineEdit PlayBackSpeedLineEdit;
    [Export] public CheckButton ClearFolderCheckBtn;
    [Export] public TextureRect _pixelShaderTextRect;
    [Export] public ModelPositionManager ModelPositionManager;
    [Export] public Button LoadAllAnimationsBtn;
    [Export] public ItemListCheckBox AnimSelectionItemList;
    [Export] public ItemListCheckBox AngleSelectionItemList;
    [Export] public Control PixelGridTextRect;
    [Export] public CheckButton ShowGridCheckButton;
    [Export] public PanelContainer MeshReplacerPanelParentNode;
    [Export] public OptionButton HairMeshOptBtn;
    [Export] public OptionButton WeaponItemMeshOptBtn;
    [Export] public ColorPickerButton HairColorBtn;

    private Button PlaySelectedAnimationsBtn => field ??= GetNodeOrNull<Button>("%PlaySelectedAnimationsBtn");

    private Node3D _modelPivotNode;
    private Node3D _modelObjectNode;
    private Camera3D _camera;
    private AnimationPlayer _animationPlayer;

    public int SpriteSize = 256;
    public int PixelResolution = 256;
    public int[] SelectedAngles;
    private readonly int MaxRBGLevelsColorPalette = 24;
    private readonly int[] allAngles = { 0, 45, 90, 135, 180, 225, 270, 315 };
    private int renderAngle = 0;
    private string currentAnimation;
    private string currentAnimationName;
    private int frameIndex;
    private int spriteCount = 1;
    private int spriteSheetCollumnCount = 8;
    private string saveFolder = "Model";

    private bool IsAnimMethod = true;
    private bool IsGenSpriteSheetOn = true;

    private bool AutoScaleModel = true;

    public override void _Ready()
    {

        //Pixel2DTest.Pressed += () => Pixel2DShaderPanel.Visible = !Pixel2DShaderPanel.Visible;
        StartGenerationBtn.Pressed += OnStartGeneration;
        SpriteSizeOptionButton.ItemSelected += OnSpriteSizeChanged;
        PixelationLevelOptionBtn.ItemSelected += OnPixelationLevelChanged;
        EffectsChoicesOptionBtn.ItemSelected += OnEffectsChoiceItemSelected;
        Outline3DValueSlider.ValueChanged += OnOutlineValuesChanged;
        Outline3DBlendFactorSlider.ValueChanged += OnOutlineValuesChanged;
        Outline3DColorPicker.ColorChanged += OnOutline3DColorChanged;
        AutoScaleModelCheckButton.ButtonPressed = AutoScaleModel;
        AutoScaleModelCheckButton.Pressed += () => AutoScaleModel = AutoScaleModelCheckButton.ButtonPressed;
        AutoScaleModelCheckButton.Text = AutoScaleModelCheckButton.ButtonPressed.ToString();
        PlaySelectedAnimationsBtn.Pressed += OnPlaySelectedAnimationsPressed;

        DepthlineThicknessSlider.ValueChanged += OnDepthlineValuesChanged;
        DepthBlendValueSlider.ValueChanged += OnDepthlineValuesChanged;
        DepthThresholdSlider.ValueChanged += OnDepthlineValuesChanged;
        Depthline3DColorPicker.ColorChanged += OnDepthlineColorChanged;

        DitheringStrenghtSlider.ValueChanged += OnDitheringStrenghtChanged;

        EnableHairMeshCheckBtn.Pressed += OnEnableHairMeshCheckBtnPressed;
        FrameStepTextEdit.TextChanged += OnFrameStepChanged;
        PlayBackSpeedLineEdit.TextChanged += OnPlayBackSpeedChanged;
        ClearFolderCheckBtn.Pressed += OnClearFolderPressed;
        LoadAllAnimationsBtn.Pressed += OnLoadAllAnimationsPressed;
        ShowGridCheckButton.Pressed += OnShowGridCheckButtonPressed;
        MaxColorPaletteSpinBox.ValueChanged += (value) => UpdateColorReductionShader();
        AnimMethodOptionBtn.ItemSelected += OnAnimMethodOptionBtnItemSelected;
        GenerateSpriteSheetCheckBtn.Pressed += OnGenerateSpriteSheetCheckBtnPressed;
        //ShowMeshPanelCheckBtn.Pressed += () => MeshOptionsMarginCont.Visible = !ShowMeshPanelCheckBtn.ButtonPressed;
        HairColorBtn.ColorChanged += OnHairColorChanged;
        HairMeshOptBtn.ItemSelected += OnHairMeshOptBtnItemSelected;
        WeaponItemMeshOptBtn.ItemSelected += OnWeaponItemMeshOptBtnItemSelected;

        ModelTypeOptionButton.ItemSelected += OnModelTypeSelected;
        LoadExternalModelBtn.Pressed += OnLoadExternalModelBtnPressed;

        //Set Default UI Control Valuesa
        ClearFolderCheckBtn.ButtonPressed = ClearFolderBeforeGeneration;
        FrameStepTextEdit.Text = FrameSkipStep.ToString();
        PlayBackSpeedLineEdit.Text = _animationPlaybackSpeed.ToString();
        AngleSelectionItemList.CreateItemsFromList(allAngles.Select(x => x.ToString()).ToArray());
        ShowGridCheckButton.ButtonPressed = false;

        //Set Default Resolution and Shader Strenght
        SpriteSizeOptionButton.Selected = SpriteSizeOptionButton.ItemCount - 2; //Select the second to last Option
        OnSpriteSizeChanged(SpriteSizeOptionButton.Selected);
        PixelationLevelOptionBtn.Selected = PixelationLevelOptionBtn.ItemCount - 2; //Select the second to last Option
        OnPixelationLevelChanged(PixelationLevelOptionBtn.Selected);
        EffectsChoicesOptionBtn.Selected = 0;
        OnEffectsChoiceItemSelected(EffectsChoicesOptionBtn.Selected);
        PixelShaderTextRect.Visible = true;
        OnPixelationLevelChanged(5);
        PixelationLevelOptionBtn.Visible = true;
        Outline3DValueSlider.Value = 0.0f;
        AnimMethodOptionBtn.Selected = 0;
        IsGenSpriteSheetOn = false;
        LoadExternalModelBtn.Visible = false;

        MaxColorPaletteSpinBox.MaxValue = MaxRBGLevelsColorPalette;
        MaxColorPaletteSpinBox.Value = MaxRBGLevelsColorPalette;
        MaxColorPaletteSpinBox.MinValue = 1;

        GlobalUtil.SaveFolderPath = ProjectSettings.GlobalizePath(Const.SAVE_GAME_PATH);

        //Load the objects from MainScene3D to the SpriteGenerator script
        LoadAllAngles();
        LoadAndPrepareModelNodes();
        UpdateViewPorts();
    }


    /// <summary>
    /// Loads and prepares the Model key nodes and required references like the PivotNode, Camera, AnimationPlayer, etc.
    /// Also loads the replaceable parts (Hair and Weapon meshes) and sets them up in the UI with default values.
    /// </summary>
    private void LoadAndPrepareModelNodes()
    {
        //LOAD MODEL KEY NODES and RQEUIRED REFERENCES
        if (MainModelScene != null)
        {
            //Get Reference to Our Object3D within MainScene and Load it's key nodes
            _modelPivotNode = MainModelScene.GetNodeOrNull<Node3D>("%Model3DPivotControl");
            _camera = MainModelScene.GetNodeOrNull<Camera3D>("%MainCamera");
            _modelObjectNode = _modelPivotNode.GetChildOrNull<Node3D>(0);
            _animationPlayer = _modelObjectNode.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");

            if (_animationPlayer == null)
            {
                _animationPlayer = _modelObjectNode.GetNodeOrNull<AnimationPlayer>("%AnimationPlayer");
            }

            //Pass the Model to te PositionManager 
            ModelPositionManager.ModelPivotNode = _modelPivotNode;
            ModelPositionManager.CameraNode = _camera;

            var modelXAxisSize = MeshReplacer.GetModelMergedAABBMeshes(_modelPivotNode).Size.Y;
            ModelPositionManager.SetTransformValueToModel(AutoScaleModel, modelXAxisSize);

            Outline3DShaderMesh = MainModelScene.GetNodeOrNull<MeshInstance3D>("%Outline3DShaderMesh");
            Depthline3DShaderMesh = MainModelScene.GetNodeOrNull<MeshInstance3D>("%Depthline3DShaderMesh");
        }
        else
        {
            Log.Error("MainScene3D is null");
        }

        //LOAD REPLACEABLE PARTS
        if (ModelHasReplacebleParts())
        {
            //Load and add Hair and Weapon Meshes to UI Buttons
            HairMeshOptBtn.Disabled = false;
            MeshReplacer.UpdateUIOptionsSceneItemList(HairMeshOptBtn, Const.HAIR_SCENES_FOLDER_PATH);
            WeaponItemMeshOptBtn.Disabled = false;
            MeshReplacer.UpdateUIOptionsSceneItemList(WeaponItemMeshOptBtn, Const.WEAPON_SCENES_FOLDER_PATH);

            //Load all MeshReplace Buttons with Model Meshes
            LoadAllMeshReplacerButtons();

            //Set default selected values
            HairMeshOptBtn.Selected = 0;
            OnHairMeshOptBtnItemSelected(0);
            HairColorBtn.Color = Colors.White;
        }
        else
        {
            ClearAllMeshReplacerButtons();
            //Log.Debug("Mode has no replaceable parts or is null");
        }
    }

    private void OnStartGeneration()
    {
        if (!GlobalUtil.HasDirectory(GlobalUtil.SaveFolderPath, this).Result) return;

        SelectedAngles = AngleSelectionItemList.GetSelectedItems().Select(x => Convert.ToInt32(AngleSelectionItemList.
        GetItemText(x))).ToArray();

        // SelectedAngles = GlobalUtil.GetGodotArrayFromGenericList<int>(_angleSelectionItemList.GetSelectedItems().ToList());

        if (SelectedAngles.Length == 0)
        {
            Log.Error("No Angles Selected");
            return;
        }

        spriteCount = 0;
        saveFolder = ProjectSettings.GlobalizePath(GlobalUtil.SaveFolderPath + "/" + _modelObjectNode.Name);

        if (!Directory.Exists(ProjectSettings.GlobalizePath(saveFolder)))
            Directory.CreateDirectory(ProjectSettings.GlobalizePath(saveFolder));


        //Log.Debug("Start Generation");

        if (ClearFolderBeforeGeneration)
            ClearFolder(saveFolder);

        PixelGridTextRect.Visible = false;

        if (IsAnimMethod)
        {
            GenerateSpritesAnimPlayerBased(SelectedAngles);
        }
        else
        {
            GenerateSpriteYRotationBased(SelectedAngles);
        }

    }

    private void ClearFolder(string folder)
    {
        string[] files = Directory.GetFiles(folder);
        foreach (string file in files)
        {
            File.Delete(file);
        }
    }

    private void OnPlaySelectedAnimationsPressed()
    {
        SelectedAngles = AngleSelectionItemList.GetSelectedItems().Select(x => Convert.ToInt32(AngleSelectionItemList.
        GetItemText(x))).ToArray();

        GenerateSpritesAnimPlayerBased(SelectedAngles, false);
    }

    private async void GenerateSpritesAnimPlayerBased(int[] selectedAngles, bool saveSprites = true)
    {
        int[] selectedAnimations = AnimSelectionItemList.GetSelectedItems();

        if (selectedAngles.Length == 0)
        {
            Log.Error("No Angles Selected");
            return;
        }

        if (selectedAnimations.Length == 0)
        {
            Log.Error("No Aniimations Selected");
            return;
        }

        _animationPlayer.Stop();
        _animationPlayer.SpeedScale = _animationPlaybackSpeed;
        //Engine.TimeScale = _animationPlaybackSpeed;

        _modelPivotNode.RotationDegrees = new Vector3(0, 0, 0);

        foreach (var selectedAnimItem in AnimSelectionItemList.GetSelectedItems())
        {
            string anim = AnimSelectionItemList.GetItemText(selectedAnimItem);

            if (anim == "RESET" || anim == "TPose") continue;

            currentAnimation = anim;
            currentAnimationName = anim.Replace("/", "_");

            foreach (var angle in selectedAngles)
            {
                _modelPivotNode.RotationDegrees = new Vector3(0, angle, 0);

                //await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw); //Testing

                _animationPlayer.Play(anim);
                Animation animationResource = _animationPlayer.GetAnimation(anim);

                float frameCount = (int)Math.Round((animationResource.Length / animationResource.Step), MidpointRounding.AwayFromZero); // Number of frames in the animation
                int framesToRender = (int)Math.Round((frameCount / FrameSkipStep), MidpointRounding.AwayFromZero);

                //Log.Debug($"FrameBASEDv2 FLOAT = animFrames: {(animationResource.Length / animationResource.Step):0.00}, will render: {(frameCount / FrameSkipStep):0.00} frames");

                float frameInterval = animationResource.Step;

                // Log.Debug($"FrameBASEDv2 = frameInterval {frameInterval:0.000}");
                float currentTime = 0f;
                int currentFrame = 0;

                // while (currentTime < animationResource.Length)
                while (_animationPlayer.IsPlaying())
                {

                    await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                    _animationPlayer.Seek(currentTime, true); // Seek to the exact time

                    //Only capture frames where frameIndex is a multiple of frameStep
                    if (currentFrame % FrameSkipStep == 0)
                    {
                        if (saveSprites)
                        {
                            await SaveFrameAsPngImg((
                                    (float)_animationPlayer.CurrentAnimationPosition).ToString("0.000"),
                                    currentAnimationName,
                                    angle);
                        }

                    }
                    currentTime += frameInterval;
                    currentFrame++;
                }
            }
            _modelPivotNode.RotationDegrees = new Vector3(0, 0, 0);
        }

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        if (IsGenSpriteSheetOn && saveSprites)
        {
            GenerateSpriteSheet(saveFolder, currentAnimationName + "_spriteSheet", spriteSheetCollumnCount);
        }
        else
        {
            PixelGridTextRect.Visible = true;
            ShowGridCheckButton.ButtonPressed = true;
        }

    }

    private async void GenerateSpriteYRotationBased(int[] selectedAngles)
    {

        foreach (var angle in selectedAngles)
        {
            _modelPivotNode.RotationDegrees = new Vector3(0, angle, 0);
            ModelPositionManager.RotationYAxisLineTextEdit.Text = _modelPivotNode.RotationDegrees.Y.ToString("0.0");

            await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw); //Testin

            await SaveFrameAsPngImg("", _modelObjectNode.Name, angle);

            //Log.Debug($"Y.Axis Sprite Generate: {_characterModelObject.Name}, angle: {angle} frames");
        }


        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        if (IsGenSpriteSheetOn)
        {
            GenerateSpriteSheet(saveFolder, _modelObjectNode.Name + "_spriteSheet", spriteSheetCollumnCount);
        }
        else
        {
            PixelGridTextRect.Visible = true;
            ShowGridCheckButton.ButtonPressed = true;
        }
    }

    private void GenerateSpriteSheet(string folderPath, string outputFileName, int columns)
    {
        //Overwite the default SpriteSheetName with the user input, if there is one.
        if (!string.IsNullOrEmpty(SpriteSheetNameTextEdit.Text))
        {
            outputFileName = SpriteSheetNameTextEdit.Text;
        }

        var dirPath = ProjectSettings.GlobalizePath(folderPath);
        if (!Directory.Exists(dirPath))
        {
            Log.Error("Directory does not exist: " + dirPath);
            return;
        }

        var imagePaths = Directory.GetFiles(dirPath, "*.png");
        if (imagePaths.Length == 0)
        {
            Log.Error("No PNG images found in: " + dirPath);
            return;
        }

        // Load first image to get dimensions
        var firstImage = Image.LoadFromFile(imagePaths[0]);
        if (firstImage == null)
        {
            Log.Error("Failed to load image: " + imagePaths[0]);
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
        string outputPath = ProjectSettings.GlobalizePath(GlobalUtil.SaveFolderPath + outputFileName + ".png");
        spriteSheet.SavePng(outputPath);

        Log.Debug("Sprite sheet saved: " + outputPath);

        PixelGridTextRect.Visible = true;

        if (MoveSpriteSheetCheckButton.ButtonPressed == true)
        {
            MoveSpriteSheetToImgEditor(spriteSheet);
        }
    }

    private void MoveSpriteSheetToImgEditor(Image imageToMove)
    {
        GlobalEvents.Instance.OnSpriteSheetCreated?.Invoke(imageToMove);
    }

    private async Task SaveFrameAsPngImg(string animPosition, string animName, int angle)
    {
        //Get the Frame and Path and File names
        //string currentAnimPosInSec = ((float)_animationPlayer.CurrentAnimationPosition).ToString("0.000");
        string currentAnimPosInSec = animPosition;
        string path = $"{saveFolder}/{animName}_{"angle_" + angle}_{spriteCount}.png";
        string globalSavePath = ProjectSettings.GlobalizePath(path);

        //Log.Debug("Saving PNG :", spriteCount + "  - FileName: " + Path.GetFileNameWithoutExtension(globalSavePath) + "  / FullPath: " + path);

        UpdateColorReductionShader();

        // Wait for the shader to be fully applied
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw); //Make sure image is updated from Shader

        //Get the Image from the given Frame / From the ViewPort and SAVE IT
        Image img = ImgColorReductionSubViewport.GetTexture().GetImage();
        img.SavePng(globalSavePath);

        spriteCount++;
        frameIndex++;

    }

    private void OnGenerateSpriteSheetCheckBtnPressed()
    {
        IsGenSpriteSheetOn = GenerateSpriteSheetCheckBtn.ButtonPressed;
        //Log.Debug("Generate Sprite Sheet: " + IsGenSpriteSheetOn);
    }

    private void UpdateColorReductionShader()
    {
        if (ImgColorReductionTextRect.Material is not ShaderMaterial shaderMaterial)
        {
            Log.Error("Material is not a ShaderMaterial.");
            return;
        }

        // Apply Shader
        shaderMaterial.SetShaderParameter("levels", (int)MaxColorPaletteSpinBox.Value);

        //Levels to colors
        // 2 levels = 8 colors
        // 4 levels = 64 colors
        // 8 levels = 512 colors
        // 16 levels = 4096 colors
        // 256 (default 8-bit) levels = 16,777,216 (Full RGB)


    }

    private void OnAnimMethodOptionBtnItemSelected(long index)
    {
        if (index == 0)
        {
            IsAnimMethod = true;
        }
        else
        {
            IsAnimMethod = false;
        }

    }

    private void OnSpriteSizeChanged(long itemSelectedIndex)
    {

        SpriteSize = itemSelectedIndex switch
        {
            0 => 64,
            1 => 128,
            2 => 256,
            3 => 384,
            4 => 512,
            5 => 768,
            _ => SpriteSize
        };

        //switch (itemSelectedIndex)
        //{
        //    case 0:
        //        SpriteSize = 64;
        //        break;
        //    case 1:
        //        SpriteSize = 128;
        //        break;
        //    case 2:
        //        SpriteSize = 256;
        //        break;
        //    case 3:
        //        SpriteSize = 384;
        //        break;
        //    case 4:
        //        SpriteSize = 512;
        //        break;
        //    case 5:
        //        SpriteSize = 768;
        //        break;

        //}
        //Log.Debug("Sprite Size/Res: " + SpriteSize);
        UpdateViewPorts();
        GlobalEvents.Instance.OnCamResetChanges?.Invoke();
        GlobalEvents.Instance.OnSpriteSizeChanged?.Invoke(SpriteSize);



    }


    private async void OnModelTypeSelected(long itemSelected)
    {
        switch (itemSelected)
        {
            case 0: //Voxel LowPoly Model
                await LoadModel(Const.Models.LOW_POLY);
                LoadExternalModelBtn.Visible = false;
                break;
            case 1: //Godot Plush (Example)
                await LoadModel(Const.Models.GODOT_PLUSH);
                LoadExternalModelBtn.Visible = false;
                break;
            case 2:
                await LoadModel(Const.Models.BARBARIAN);
                LoadExternalModelBtn.Visible = false;
                break;
            case 3:
                await LoadModel(Const.Models.MEDIUM_POLY);
                LoadExternalModelBtn.Visible = false;
                break;
            case 4: //Custom model //TODO: Exprimental with Leak of memory
                LoadExternalModelBtn.Visible = true;
                break;
        }
    }

    private async void OnLoadExternalModelBtnPressed()
    {
        await SelectExternalGLTFModel();
    }

    private async Task SelectExternalGLTFModel()
    {
        var dialog = new FileDialog();

        using Godot.FileDialog fileDialog = new Godot.FileDialog
        {
            FileMode = FileDialog.FileModeEnum.OpenFile,
            Filters = new string[] { "*.gltf, *.glb ; GLTF File" },
            Access = FileDialog.AccessEnum.Filesystem
        };

        AddChild(fileDialog);

        fileDialog.CurrentDir = ProjectSettings.GlobalizePath(Const.USER_ROOT_FOLDER_PATH);

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        fileDialog.PopupCentered();

        await ToSignal(fileDialog, FileDialog.SignalName.FileSelected);

        string fullFilePath = fileDialog.CurrentDir + "/" + fileDialog.CurrentFile;

        await LoadModel(fullFilePath, true);

        RemoveChild(fileDialog);


    }

    private async Task LoadModel(string modelScenePath, bool isExternalModel = false)
    {
        //Prepare and Reset Positions for New Model
        ModelPositionManager.CameDistance = 80.0f;
        ModelPositionManager.CamDistancelLineTextEdit.Text = "80.0";
        ModelPositionManager.ModelPosition = new Vector3(0.0f, 0.0f, 0.0f);
        ModelPositionManager.ModelRotation = new Vector3(0.0f, 0.0f, 0.0f);
        ModelPositionManager.SetTransformValueToModel(false);

        //first we remove the current loaded scene from the PivotParentNode
        var loadedModelScene = _modelPivotNode.GetChild(0);

        if (loadedModelScene != null)
        {
            _modelPivotNode.RemoveChild(loadedModelScene);
            loadedModelScene.QueueFree();
        }

        //Second: We load the new scene anad handle logic if External or BuiltInmodels
        if (isExternalModel)
        {
            //The LoadExternalGLTF function will take care of Adding it as child, etc. 
            // await GLTFLoader.LoadExternalGLTF(modelScenePath, _modelPivotNode);
            Node3D newModelInstance = (Node3D)await GLTFLoader.LoadExternalGLTF(modelScenePath, _modelPivotNode);
            _modelPivotNode.AddChild(newModelInstance);

        }
        else
        {
            PackedScene newModelScene = GD.Load<PackedScene>(modelScenePath);
            Node3D newModelInstance = newModelScene.Instantiate<Node3D>();
            _modelPivotNode.AddChild(newModelInstance);
        }

        LoadAndPrepareModelNodes();
    }


    private void OnEnableHairMeshCheckBtnPressed()
    {
        var hairBoneNode = _modelObjectNode.GetNodeOrNull<BoneAttachment3D>("%HairBoneAttach");
        if (hairBoneNode != null)
        {

            hairBoneNode.Visible = EnableHairMeshCheckBtn.ButtonPressed;
        }
    }


    private void OnEffectsChoiceItemSelected(long itemSelectedIndex)
    {

        //Log.Debug("Effect Selected: " + itemSelectedIndex);
        switch (itemSelectedIndex)
        {
            //TODO: Apply different effects. Create an EffetHandler? Change the materials for the settings
            //Option 1 -> Unshaaded 
            //Option 2 -> Toon Shading
            case 0:
                //No Effect - Turn off PixaltionButton
                OnPixelationLevelChanged(5);//Set the resolution to 512 pixels (Last option)
                Callable.From(() => EffectsHandler.SetEffect(_modelObjectNode, Const.EffectShadingType.STANDARD)).CallDeferred();
                //EffectsHandler.SetEffect(_characterModelObject, Const.EffectShadingType.STANDARD);//TODO: TESTING ONLY
                break;
            case 1:
                //Pixel Effect
                OnPixelationLevelChanged(PixelationLevelOptionBtn.Selected);
                EffectsHandler.SetEffect(_modelObjectNode, Const.EffectShadingType.UNSHADED);//TODO: TESTING ONLY
                break;
            case 2:
                //Toon Effect
                OnPixelationLevelChanged(PixelationLevelOptionBtn.Selected);
                EffectsHandler.SetEffect(_modelObjectNode, Const.EffectShadingType.TOON);//TODO: TESTING ONLY
                break;
        }
        UpdateViewPorts();
    }

    private void OnPixelationLevelChanged(long itemSelectedIndex)
    {
        PixelResolution = itemSelectedIndex switch
        {
            0 => 32,
            1 => 64,
            2 => 128,
            3 => 256,
            4 => 384,
            5 => 512,
            6 => 768,
            _ => PixelResolution
        };

        //switch (itemSelectedIndex)
        //{
        //    case 0:
        //        PixelResolution = 32;
        //        break;
        //    case 1:
        //        PixelResolution = 64;
        //        break;
        //    case 2:
        //        PixelResolution = 128;
        //        break;
        //    case 3:
        //        PixelResolution = 256;
        //        break;
        //    case 4:
        //        PixelResolution = 384;
        //        break;
        //    case 5:
        //        PixelResolution = 512;
        //        break;
        //    case 6:
        //        PixelResolution = 768;
        //        break;

        //Log.Debug("Effect Pixel Resolution: " + SpriteSize);


        if (PixelShaderTextRect.Material is ShaderMaterial shaderMaterial)
        {
            shaderMaterial.SetShaderParameter("target_resolution", PixelResolution);
        }
        UpdateViewPorts();
    }


    private void OnOutlineValuesChanged(double value)
    {
        //Trace.Assert(Outline3DShaderMesh != null, "Outline3DShaderMesh cannot be null");
        if (Outline3DShaderMesh == null) return;

        if (Outline3DShaderMesh.Mesh.SurfaceGetMaterial(0) is ShaderMaterial shaderMaterial)
        {
            shaderMaterial.SetShaderParameter("outline_width", Outline3DValueSlider.Value);
            shaderMaterial.SetShaderParameter("outline_colorblend_factor", Outline3DBlendFactorSlider.Value);
        }
    }

    private void OnOutline3DColorChanged(Color color)
    {
        Trace.Assert(Outline3DShaderMesh != null, "Outline3DShaderMesh cannot be null");
        if (Outline3DShaderMesh == null) return;

        if (Outline3DShaderMesh.Mesh.SurfaceGetMaterial(0) is ShaderMaterial shaderMaterial)
        {
            shaderMaterial.SetShaderParameter("outline_fallback_color", color);
        }
    }

    private void OnDepthlineValuesChanged(double value)
    {
        Trace.Assert(Depthline3DShaderMesh != null, "Depthline3DMesh cannot be null");
        if (Depthline3DShaderMesh == null) return;

        if (Depthline3DShaderMesh.Mesh.SurfaceGetMaterial(0) is ShaderMaterial shaderMaterial)
        {
            shaderMaterial.SetShaderParameter("depthline_thickness", DepthlineThicknessSlider.Value);
            shaderMaterial.SetShaderParameter("colorblend_factor", DepthBlendValueSlider.Value);

            int depthThresholdValue = (int)DepthThresholdSlider.Value;
            switch (depthThresholdValue)
            {
                case 0:
                    shaderMaterial.SetShaderParameter("depth_sensitivity", 0);
                    shaderMaterial.SetShaderParameter("normal_sensitivity", 0);
                    break;
                case 1:
                    shaderMaterial.SetShaderParameter("depth_sensitivity", 1.0);
                    shaderMaterial.SetShaderParameter("normal_sensitivity", 0.0);
                    break;
                case 2:
                    shaderMaterial.SetShaderParameter("depth_sensitivity", 2.0);
                    shaderMaterial.SetShaderParameter("normal_sensitivity", 0.0);
                    break;
                case 3:
                    shaderMaterial.SetShaderParameter("depth_sensitivity", 2.0);
                    shaderMaterial.SetShaderParameter("normal_sensitivity", 0.01);
                    break;
                case 4:
                    shaderMaterial.SetShaderParameter("depth_sensitivity", 3.0);
                    shaderMaterial.SetShaderParameter("normal_sensitivity", 0.02);
                    break;
                case 5:
                    shaderMaterial.SetShaderParameter("depth_sensitivity", 5.0);
                    shaderMaterial.SetShaderParameter("normal_sensitivity", 3.0);
                    break;
                default:
                    break;
            }
        }
    }

    private void OnDepthlineColorChanged(Color color)
    {
        Trace.Assert(Depthline3DShaderMesh != null, "Depthline3DMesh cannot be null");
        if (Depthline3DShaderMesh == null) return;

        if (Depthline3DShaderMesh.Mesh.SurfaceGetMaterial(0) is ShaderMaterial shaderMaterial)
        {
            shaderMaterial.SetShaderParameter("depthline_color", color);
        }
    }


    private void OnDitheringStrenghtChanged(double value)
    {
        if (ImgColorReductionTextRect?.Material is ShaderMaterial shaderMaterial)
        {
            shaderMaterial.SetShaderParameter("dither_strength", value);
        }
    }


    private void OnPlayBackSpeedChanged(string newText)
    {
        if (string.IsNullOrEmpty(newText)) return;

        _animationPlaybackSpeed = Convert.ToInt32(newText);
        //Log.Debug("PlaybackSpeed: " + _animationPlaybackSpeed);

    }

    private void UpdateViewPorts()
    {
        Vector2I viewPortSize = new Vector2I(SpriteSize, SpriteSize);
        RawViewport?.CallDeferred("set_size", viewPortSize);
        _rawViewportContainer?.CallDeferred("set_size", viewPortSize);

        // DisplaySubViewPort?.CallDeferred("set_size", viewPortSize);
        //MainRenderPanel.CallDeferred("set_size", viewPortSize);

        BgRemoverViewport?.CallDeferred("set_size", viewPortSize);
        BgRemoverViewportContainer?.CallDeferred("set_size", viewPortSize);

        ImgColorReductionSubViewport?.CallDeferred("set_size", viewPortSize);
        ImgColorReductionSubViewportContainer?.CallDeferred("set_size", viewPortSize);

        Callable.From(() =>
            PixelSmoothEffectSprite2D.Position = new Vector2((viewPortSize.X / 2), (viewPortSize.Y / 2))
            ).CallDeferred();



        //Callable.From(() => ImgColorReductionSubViewport.SetSize(viewPortSize)).CallDeferred();
    }
    private void OnClearFolderPressed()
    {
        ClearFolderBeforeGeneration = ClearFolderCheckBtn.ButtonPressed;
        //Log.Debug("Clear Folder: " + ClearFolderBeforeGeneration);
    }


    private void OnFrameStepChanged(string newText)
    {
        if (string.IsNullOrEmpty(newText)) return;

        FrameSkipStep = Convert.ToInt32(newText);
        //Log.Debug("Frame Step: " + FrameSkipStep);
    }

    private void OnShowGridCheckButtonPressed()
    {
        PixelGridTextRect.Visible = ShowGridCheckButton.ButtonPressed;

    }

    private void OnLoadAllAnimationsPressed()
    {
        AnimSelectionItemList?.Clear();

        foreach (var animationItem in _animationPlayer.GetAnimationList())
        {
            if (animationItem == "RESET" || animationItem == "TPose") continue;
            AnimSelectionItemList?.AddItem(animationItem, AnimSelectionItemList.ICON_UNSELECTED, true);

        }
    }

    private void OnHairColorChanged(Color newColor)
    {
        MeshInstance3D _hairMeshObject = _modelObjectNode.GetNode<BoneAttachment3D>("%HairBoneAttach").GetChild(0).GetNode<MeshInstance3D>("%HairMesh");

        if (_hairMeshObject != null && _hairMeshObject.GetActiveMaterial(0) is StandardMaterial3D material)
        {
            material.AlbedoColor = newColor;
        }
    }

    private void OnHairMeshOptBtnItemSelected(long index)
    {
        BoneAttachment3D _hairBoneAttachNode = _modelObjectNode.GetNode<BoneAttachment3D>("%HairBoneAttach");
        string itemSelected = HairMeshOptBtn.GetItemText((int)index);
        MeshReplacer.UpdateMeshScene(_hairBoneAttachNode, Const.HAIR_SCENES_FOLDER_PATH + itemSelected + ".tscn");
    }


    private void OnWeaponItemMeshOptBtnItemSelected(long index)
    {
        BoneAttachment3D weaponBoneAttachNode = _modelObjectNode.GetNode<BoneAttachment3D>("%WeaponBoneAttach");
        string itemSelected = WeaponItemMeshOptBtn.GetItemText((int)index);
        MeshReplacer.UpdateMeshScene(weaponBoneAttachNode, Const.WEAPON_SCENES_FOLDER_PATH + itemSelected + ".tscn");
    }


    private void LoadAllMeshReplacerButtons()
    {

        var allMeshReplacerOptButtons = GlobalUtil.GetAllChildNodesByType<MeshReplacerOptButton>(MeshReplacerPanelParentNode);
        //Log.Debug("Trying to load " + allMeshReplacerOptButtons.Count + " MeshReplacerOptButtons");

        foreach (var meshReplacerOptButton in allMeshReplacerOptButtons)
        {
            //Connecct the signal and load items
            meshReplacerOptButton.ItemSelected += (itemIndex) => OnMeshItemSelected(itemIndex, meshReplacerOptButton);
            MeshReplacer.UpdateUIOptionMesheItemList(meshReplacerOptButton);
            meshReplacerOptButton.Disabled = false;

            //Align UI display to the first item
            //TODO: Uncomment line belows to force the model to load the first item
            //meshReplacerOptButton.Selected = 0;
            //OnMeshItemSelected(0, meshReplacerOptButton);

            OnMeshItemSelected(0, meshReplacerOptButton);
        }

    }

    private void ClearAllMeshReplacerButtons()
    {
        var allMeshReplacerOptButtons = GlobalUtil.GetAllChildNodesByType<MeshReplacerOptButton>(MeshReplacerPanelParentNode);
        //Log.Debug("Found " + allMeshReplacerOptButtons.Count + " MeshReplacerOptButton");

        foreach (var meshReplacerOptButton in allMeshReplacerOptButtons)
        {
            meshReplacerOptButton.Clear();
            meshReplacerOptButton.Disabled = true;
        }
        HairMeshOptBtn.Clear();
        HairMeshOptBtn.Disabled = true;
        WeaponItemMeshOptBtn.Clear();
        WeaponItemMeshOptBtn.Disabled = true;
    }

    private void OnMeshItemSelected(long itemIndex, MeshReplacerOptButton meshReplacerOptButton)
    {

        string itemSelectedName = meshReplacerOptButton.GetItemText((int)itemIndex);
        //Log.Debug("Mesh Item Selected: " + itemSelectedName + " + FromButton: " + meshReplacerOptButton.Name);

        var _targetMeshInstance = GlobalUtil.GetAllChildNodesByType<BodyPartMeshInstance3D>(_modelObjectNode).
            Where(bodyPartMesh => bodyPartMesh.BodyPartType == meshReplacerOptButton.BodyPartType).FirstOrDefault();

        if (_targetMeshInstance == null)
        {
            Log.Error("MeshInstance3D not found for BodyPartType: " + meshReplacerOptButton.BodyPartType);
            return;
        }

        ArrayMeshDataObject meshDataObjtSelected = MeshReplacer.GetArrayMeshDataObject(itemSelectedName);

        if (meshDataObjtSelected.CanChangeColor)
        {
            meshReplacerOptButton.EnableColorPicker(true);
        }
        else
        {
            meshReplacerOptButton.EnableColorPicker(false);
        }

        //Make sure we will show or hide the Mesh Color button by 
        MeshReplacer.UpdateMeshFromResourceItem(_targetMeshInstance, itemSelectedName, meshReplacerOptButton.BodyPartType);

        //Make sure the Mesh will have the effect selected in the Effect Choice Option
        OnEffectsChoiceItemSelected(EffectsChoicesOptionBtn.Selected);
    }

    private bool ModelHasReplacebleParts()
    {
        //Check if the model has at least one replaceable part
        if (GlobalUtil.GetAllChildNodesByType<BodyPartMeshInstance3D>(_modelObjectNode).FirstOrDefault() != null)
        {
            return true;
        }

        return false;
    }

    //Change the Mesh base based on the MeshReplacaeOpt button selection (Connected signal)

    public void LoadAllAngles(int[] selectedAngleIndex = null)
    {
        AngleSelectionItemList.Clear();

        foreach (int angle in allAngles)
        {
            AngleSelectionItemList.AddItem(angle.ToString(), AngleSelectionItemList.ICON_UNSELECTED, true);
        }

        if (selectedAngleIndex == null) return;

        foreach (int selectedAngle in selectedAngleIndex)
        {
            AngleSelectionItemList.Select(selectedAngle);
            AngleSelectionItemList.SetItemIcon(selectedAngle, AngleSelectionItemList.ICON_SELECTED);
        }
    }

    public void OnSaveData(SaveGameData newSaveGameData)
    {
        //Log.Debug("Started OnSaveData from:", this.Name);
    }

    public void OnLoadData(SaveGameData newLoadData)
    {
        //Log.Debug("Started OnLoadData from:", this.Name);


        UpdateViewPorts();
        //OnPlayBackSpeedChanged(newLoadData.PlaybackSpeed.ToString());
        OnFrameStepChanged(newLoadData.FrameSkipStep.ToString());
        OnShowGridCheckButtonPressed();
        OnEffectsChoiceItemSelected(EffectsChoicesOptionBtn.Selected);
        OnPixelationLevelChanged(newLoadData.PixelResolutionButtonSelected);
        OnSpriteSizeChanged(newLoadData.SpriteSizeOptionButtonSelected);

        ModelPositionManager.SetTransformValueToModel();
    }
}
