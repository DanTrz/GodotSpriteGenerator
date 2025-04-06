using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public partial class SpriteGenerator : Node
{
    // [Export] public Button Pixel2DTest;
    // [Export] public PanelContainer Pixel2DShaderPanel;

    [Export] public OptionButton AnimMethodOptionBtn;
    [Export] public CheckButton GenerateSpriteSheetCheckBtn;
    [Export] public CheckButton ShowMeshPanelCheckBtn;
    [Export] public MarginContainer MeshOptionsMarginCont;
    // [Export] public Button _startGenerationBtn;
    [Export] public Button _startGenerationBtn;
    [Export] public Node3D MainModelScene3D;
    [Export] public SubViewport _rawViewport;
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
    [Export] public int frameSkipStep = 4; // Control how frequently frames are captured
    [Export] public bool _clearFolderBeforeGeneration = true;
    [Export(PropertyHint.Range, "1,4,1")] private float _animationPlaybackSpeed = 1.0f;
    [Export] public OptionButton SpriteSizeOptionButton;
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
    [Export] public LineEdit _frameStepTextEdit;

    [Export] public CheckButton EnableHairMeshCheckBtn;

    [Export] public SpinBox MaxColorPaletteSpinBox;

    [Export] public LineEdit _playBackSpeedLineEdit;
    [Export] public CheckButton _clearFolderCheckBtn;
    [Export] public TextureRect _pixelShaderTextRect;
    [Export] public ModelPositionManager _modelPositionManager;
    [Export] public Button _loadAllAnimationsBtn;
    [Export] public ItemListCheckBox _animSelectionItemList;
    [Export] public ItemListCheckBox _angleSelectionItemList;
    [Export] public TextureRect _pixelGridTextRect;
    [Export] public CheckButton _showGridCheckButton;
    [Export] public PanelContainer _meshReplacerPanelParentNode;
    [Export] public OptionButton _hairMeshOptBtn;
    [Export] public OptionButton WeaponItemMeshOptBtn;
    [Export] public ColorPickerButton _hairColorBtn;


    private Node3D _modelPivotNode;
    private Node3D _characterModelObject;
    private Camera3D _camera;
    private AnimationPlayer _animationPlayer;

    public static int _spriteResolution = 256;

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


    public override void _Ready()
    {
        //Pixel2DTest.Pressed += () => Pixel2DShaderPanel.Visible = !Pixel2DShaderPanel.Visible;
        _startGenerationBtn.Pressed += OnStartGeneration;
        SpriteSizeOptionButton.ItemSelected += OnSpriteSizeChanged;
        PixelationLevelOptionBtn.ItemSelected += OnPixelationLevelChanged;
        EffectsChoicesOptionBtn.ItemSelected += OnEffectsChoiceItemSelected;
        Outline3DValueSlider.ValueChanged += OnOutlineValuesChanged;
        Outline3DBlendFactorSlider.ValueChanged += OnOutlineValuesChanged;
        Outline3DColorPicker.ColorChanged += OnOutline3DColorChanged;

        DepthlineThicknessSlider.ValueChanged += OnDepthlineValuesChanged;
        DepthBlendValueSlider.ValueChanged += OnDepthlineValuesChanged;
        DepthThresholdSlider.ValueChanged += OnDepthlineValuesChanged;
        Depthline3DColorPicker.ColorChanged += OnDepthlineColorChanged;

        DitheringStrenghtSlider.ValueChanged += OnDitheringStrenghtChanged;

        EnableHairMeshCheckBtn.Pressed += OnEnableHairMeshCheckBtnPressed;
        _frameStepTextEdit.TextChanged += OnFrameStepChanged;
        _playBackSpeedLineEdit.TextChanged += OnPlayBackSpeedChanged;
        _clearFolderCheckBtn.Pressed += OnClearFolderPressed;
        _loadAllAnimationsBtn.Pressed += OnLoadAllAnimationsPressed;
        _showGridCheckButton.Pressed += OnShowGridCheckButtonPressed;
        MaxColorPaletteSpinBox.ValueChanged += (value) => UpdateColorReductionShader();
        AnimMethodOptionBtn.ItemSelected += OnAnimMethodOptionBtnItemSelected;
        GenerateSpriteSheetCheckBtn.Pressed += OnGenerateSpriteSheetCheckBtnPressed;
        ShowMeshPanelCheckBtn.Pressed += () => MeshOptionsMarginCont.Visible = !ShowMeshPanelCheckBtn.ButtonPressed;
        _hairColorBtn.ColorChanged += OnHairColorChanged;
        _hairMeshOptBtn.ItemSelected += OnHairMeshOptBtnItemSelected;
        WeaponItemMeshOptBtn.ItemSelected += OnWeaponItemMeshOptBtnItemSelected;

        //Set Default UI Control Values
        _clearFolderCheckBtn.ButtonPressed = _clearFolderBeforeGeneration;
        _frameStepTextEdit.Text = frameSkipStep.ToString();
        _playBackSpeedLineEdit.Text = _animationPlaybackSpeed.ToString();
        _angleSelectionItemList.CreateItemsFromList(allAngles.Select(x => x.ToString()).ToArray());
        _showGridCheckButton.ButtonPressed = false;

        //Set Default Resolution and Shader Strenght
        SpriteSizeOptionButton.Selected = SpriteSizeOptionButton.ItemCount - 1; //Select the Last Option
        OnSpriteSizeChanged(SpriteSizeOptionButton.Selected);
        PixelationLevelOptionBtn.Selected = PixelationLevelOptionBtn.ItemCount - 1; //Last option 
        OnPixelationLevelChanged(PixelationLevelOptionBtn.Selected);
        EffectsChoicesOptionBtn.Selected = 0;
        OnEffectsChoiceItemSelected(EffectsChoicesOptionBtn.Selected);
        PixelShaderTextRect.Visible = true;
        OnPixelationLevelChanged(5);
        PixelationLevelOptionBtn.Visible = true;
        Outline3DValueSlider.Value = 0.0f;
        AnimMethodOptionBtn.Selected = 0;
        IsGenSpriteSheetOn = false;

        MaxColorPaletteSpinBox.MaxValue = MaxRBGLevelsColorPalette;
        MaxColorPaletteSpinBox.Value = MaxRBGLevelsColorPalette;
        MaxColorPaletteSpinBox.MinValue = 1;

        GlobalUtil.SaveFolderPath = ProjectSettings.GlobalizePath(Const.SAVE_GAME_PATH);

        //Load the objects from MainScene3D to the SpriteGenerator script
        LoadAndPrepareModelNodes();
        UpdateViewPorts();
    }

    private void OnEnableHairMeshCheckBtnPressed()
    {
        var hairBoneNode = _characterModelObject.GetNodeOrNull<BoneAttachment3D>("%HairBoneAttach");
        if (hairBoneNode != null)
        {

            hairBoneNode.Visible = EnableHairMeshCheckBtn.ButtonPressed;
        }
    }


    private void LoadAndPrepareModelNodes()
    {
        //LOAD MODEL KEY NODES and RQEUIRED REFERENCES
        if (MainModelScene3D != null)
        {
            //Get Reference to Our Object3D within MainScene and Load it's key nodes
            _modelPivotNode = MainModelScene3D.GetNodeOrNull<Node3D>("%Model3DPivot_AddYourModelHere");
            _camera = MainModelScene3D.GetNodeOrNull<Camera3D>("%MainCamera");
            _characterModelObject = _modelPivotNode.GetChildOrNull<Node3D>(0);
            _animationPlayer = _characterModelObject.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");

            //Pass the Model to te PositionManager 
            _modelPositionManager.ModelNode = _modelPivotNode;
            _modelPositionManager.CameraNode = _camera;
            _modelPositionManager.SetTransformValueToModel(true);

            Outline3DShaderMesh = MainModelScene3D.GetNodeOrNull<MeshInstance3D>("%Outline3DShaderMesh");
            Depthline3DShaderMesh = MainModelScene3D.GetNodeOrNull<MeshInstance3D>("%Depthline3DShaderMesh");
        }
        else
        {
            GD.PrintErr("MainScene3D is null");
        }

        //LOAD REPLACEABLE PARTS
        if (ModelHasReplacebleParts())
        {
            //Load and add Hair and Weapon Meshes to UI Buttons
            MeshReplacer.UpdateUIOptionsSceneItemList(_hairMeshOptBtn, Const.HAIR_SCENES_FOLDER_PATH);
            MeshReplacer.UpdateUIOptionsSceneItemList(WeaponItemMeshOptBtn, Const.WEAPON_SCENES_FOLDER_PATH);

            //Load all MeshReplace Buttons with Model Meshes
            LoadAllMeshReplacerBtnAndMeshItemData();

            //Set default selected values
            _hairMeshOptBtn.Selected = 0;
            OnHairMeshOptBtnItemSelected(0);
            _hairColorBtn.Color = Colors.White;
        }
        else
        {
            GD.PrintT("Mode has no replaceable parts or is null");
        }
    }

    private void OnStartGeneration()
    {
        if (!GlobalUtil.HasDirectory(GlobalUtil.SaveFolderPath, this).Result) return;

        int[] selectedAngles = _angleSelectionItemList.GetSelectedItems().Select(x => Convert.ToInt32(_angleSelectionItemList.
        GetItemText(x))).ToArray();

        if (selectedAngles.Length == 0)
        {
            GD.PrintErr("No Angles Selected");
            return;
        }

        spriteCount = 0;
        saveFolder = ProjectSettings.GlobalizePath(GlobalUtil.SaveFolderPath + "/" + _characterModelObject.Name);

        if (!Directory.Exists(ProjectSettings.GlobalizePath(saveFolder)))
            Directory.CreateDirectory(ProjectSettings.GlobalizePath(saveFolder));


        GD.PrintT("Start Generation");

        if (_clearFolderBeforeGeneration)
            ClearFolder(saveFolder);

        _pixelGridTextRect.Visible = false;

        if (IsAnimMethod)
        {
            GenerateSpritesAnimPlayerBased(selectedAngles);
        }
        else
        {
            GenerateSpriteYRotationBased(selectedAngles);
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

    private async void GenerateSpritesAnimPlayerBased(int[] selectedAngles)
    {
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

                //await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw); //Testing

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
                        //OLDCODE
                        //await SaveFrameAsImgPNG(angle);
                        //Replace with Method to add to a Queue
                        await SaveFrameAsPngImg((
                            (float)_animationPlayer.CurrentAnimationPosition).ToString("0.000"),
                            currentAnimationName,
                            angle);
                    }

                    currentTime += frameInterval;

                    currentFrame++;
                }
            }

            _modelPivotNode.RotationDegrees = new Vector3(0, 0, 0);
        }

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        if (IsGenSpriteSheetOn)
        {
            GenerateSpriteSheet(saveFolder, currentAnimationName + "_spriteSheet", spriteSheetCollumnCount);
        }
        else
        {
            _pixelGridTextRect.Visible = true;
            _showGridCheckButton.ButtonPressed = true;
        }

    }

    private async void GenerateSpriteYRotationBased(int[] selectedAngles)
    {

        foreach (var angle in selectedAngles)
        {
            _modelPivotNode.RotationDegrees = new Vector3(0, angle, 0);
            _modelPositionManager._rotationYAxisLineTextEdit.Text = _modelPivotNode.RotationDegrees.Y.ToString("0.0");

            //await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw); //Testin

            await SaveFrameAsPngImg("", _characterModelObject.Name, angle);

            GD.PrintT($"Y.Axis Sprite Generate: {_characterModelObject.Name}, angle: {angle} frames");
        }


        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        if (IsGenSpriteSheetOn)
        {
            GenerateSpriteSheet(saveFolder, _characterModelObject.Name + "_spriteSheet", spriteSheetCollumnCount);
        }
        else
        {
            _pixelGridTextRect.Visible = true;
            _showGridCheckButton.ButtonPressed = true;
        }
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
        string outputPath = ProjectSettings.GlobalizePath(GlobalUtil.SaveFolderPath + outputFileName + ".png");
        spriteSheet.SavePng(outputPath);

        GD.Print("Sprite sheet saved: " + outputPath);

        _pixelGridTextRect.Visible = true;
    }

    private async Task SaveFrameAsPngImg(string animPosition, string animName, int angle)
    {
        //Get the Frame and Path and File names
        //string currentAnimPosInSec = ((float)_animationPlayer.CurrentAnimationPosition).ToString("0.000");
        string currentAnimPosInSec = animPosition;
        string path = $"{saveFolder}/{animName}_{"angle_" + angle}_{spriteCount}.png";
        string globalSavePath = ProjectSettings.GlobalizePath(path);

        GD.PrintT("Saving PNG :", spriteCount + "  - FileName: " + Path.GetFileNameWithoutExtension(globalSavePath) + "  / FullPath: " + path);

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
        GD.PrintT("Generate Sprite Sheet: " + IsGenSpriteSheetOn);
    }

    private void UpdateColorReductionShader()
    {
        if (ImgColorReductionTextRect.Material is not ShaderMaterial shaderMaterial)
        {
            GD.PrintErr("Material is not a ShaderMaterial.");
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
                _spriteResolution = 384;
                break;
            case 4:
                _spriteResolution = 512;
                break;
                // case 5:
                //     _spriteResolution = 1024;
                //     break;

        }
        GD.PrintT("Sprite Size/Res: " + _spriteResolution);

        UpdateViewPorts();
    }


    private void OnEffectsChoiceItemSelected(long itemSelectedIndex)
    {

        GD.PrintT("Effect Selected: " + itemSelectedIndex);
        switch (itemSelectedIndex)
        {
            //TODO: Apply different effects. Create an EffetHandler? Change the materials for the settings
            //Option 1 -> Unshaaded 
            //Option 2 -> Toon Shading
            case 0:
                //No Effect - Turn off PixaltionButton
                OnPixelationLevelChanged(5);//Set the resolution to 512 pixels (Last option)
                Callable.From(() => EffectsHandler.SetEffect(_characterModelObject, Const.EffectShadingType.STANDARD)).CallDeferred();
                //EffectsHandler.SetEffect(_characterModelObject, Const.EffectShadingType.STANDARD);//TODO: TESTING ONLY
                break;
            case 1:
                //Pixel Effect
                OnPixelationLevelChanged(PixelationLevelOptionBtn.Selected);
                EffectsHandler.SetEffect(_characterModelObject, Const.EffectShadingType.UNSHADED);//TODO: TESTING ONLY
                break;
            case 2:
                //Toon Effect
                OnPixelationLevelChanged(PixelationLevelOptionBtn.Selected);
                EffectsHandler.SetEffect(_characterModelObject, Const.EffectShadingType.TOON);//TODO: TESTING ONLY
                break;
        }
        UpdateViewPorts();
    }

    private void OnPixelationLevelChanged(long itemSelectedIndex)
    {
        int pixelShaderResolution = 0;

        switch (itemSelectedIndex)
        {
            case 0:
                pixelShaderResolution = 32;
                break;
            case 1:
                pixelShaderResolution = 64;
                break;
            case 2:
                pixelShaderResolution = 128;
                break;
            case 3:
                pixelShaderResolution = 256;
                break;
            case 4:
                pixelShaderResolution = 384;
                break;
            case 5:
                pixelShaderResolution = 512;
                break;
                // case 6:
                //     pixelShaderResolution = 1024;
                //     break;



        }

        GD.PrintT("Effect Pixel Resolution: " + _spriteResolution);

        // if (MeshShaderPixel3D.Mesh.SurfaceGetMaterial(0) is ShaderMaterial shaderMaterial)
        // {
        //     shaderMaterial.SetShaderParameter("target_resolution", pixelShaderResolution);
        // }


        if (PixelShaderTextRect.Material is ShaderMaterial shaderMaterial)
        {
            shaderMaterial.SetShaderParameter("target_resolution", pixelShaderResolution);
        }
        UpdateViewPorts();
    }


    private void OnOutlineValuesChanged(double value)
    {
        Trace.Assert(Outline3DShaderMesh != null, "Outline3DShaderMesh cannot be null");
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
        if (ImgColorReductionTextRect.Material is ShaderMaterial shaderMaterial)
        {
            shaderMaterial.SetShaderParameter("dither_strength", value);
        }
    }


    private void OnPlayBackSpeedChanged(string newText)
    {
        if (string.IsNullOrEmpty(newText)) return;

        _animationPlaybackSpeed = Convert.ToInt32(newText);
        GD.PrintT("PlaybackSpeed: " + _animationPlaybackSpeed);

    }

    private void UpdateViewPorts()
    {
        Vector2I viewPortSize = new Vector2I(_spriteResolution, _spriteResolution);
        _rawViewport.CallDeferred("set_size", viewPortSize);
        _rawViewportContainer.CallDeferred("set_size", viewPortSize);

        BgRemoverViewport.CallDeferred("set_size", viewPortSize);
        BgRemoverViewportContainer.CallDeferred("set_size", viewPortSize);

        ImgColorReductionSubViewport.CallDeferred("set_size", viewPortSize);
        ImgColorReductionSubViewportContainer.CallDeferred("set_size", viewPortSize);

        Callable.From(() =>
            PixelSmoothEffectSprite2D.Position = new Vector2((viewPortSize.X / 2), (viewPortSize.Y / 2))
            ).CallDeferred();



        //Callable.From(() => ImgColorReductionSubViewport.SetSize(viewPortSize)).CallDeferred();
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

    }

    private void OnLoadAllAnimationsPressed()
    {
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
        MeshReplacer.UpdateMeshScene(_hairBoneAttachNode, Const.HAIR_SCENES_FOLDER_PATH + itemSelected + ".tscn");
    }


    private void OnWeaponItemMeshOptBtnItemSelected(long index)
    {
        BoneAttachment3D weaponBoneAttachNode = _characterModelObject.GetNode<BoneAttachment3D>("%WeaponBoneAttach");
        string itemSelected = WeaponItemMeshOptBtn.GetItemText((int)index);
        MeshReplacer.UpdateMeshScene(weaponBoneAttachNode, Const.WEAPON_SCENES_FOLDER_PATH + itemSelected + ".tscn");
    }


    private void LoadAllMeshReplacerBtnAndMeshItemData()
    {

        var allMeshReplacerOptButtons = GlobalUtil.GetAllNodesByType<MeshReplacerOptButton>(_meshReplacerPanelParentNode);
        GD.PrintT("Found " + allMeshReplacerOptButtons.Count + " MeshReplacerOptButton");

        foreach (var meshReplacerOptButton in allMeshReplacerOptButtons)
        {
            //Connecct the signal and load items
            meshReplacerOptButton.ItemSelected += (itemIndex) => OnMeshItemSelected(itemIndex, meshReplacerOptButton);
            MeshReplacer.UpdateUIOptionMesheItemList(meshReplacerOptButton);

            //Align UI display to the first item
            //TODO: Uncomment line belows to force the model to load the first item
            //meshReplacerOptButton.Selected = 0;
            //OnMeshItemSelected(0, meshReplacerOptButton);

            OnMeshItemSelected(0, meshReplacerOptButton);
        }

    }

    private void OnMeshItemSelected(long itemIndex, MeshReplacerOptButton meshReplacerOptButton)
    {

        string itemSelectedName = meshReplacerOptButton.GetItemText((int)itemIndex);
        GD.PrintT("Mesh Item Selected: " + itemSelectedName + " + FromButton: " + meshReplacerOptButton.Name);

        var _targetMeshInstance = GlobalUtil.GetAllNodesByType<BodyPartMeshInstance3D>(_characterModelObject).
            Where(bodyPartMesh => bodyPartMesh.BodyPartType == meshReplacerOptButton.BodyPartType).FirstOrDefault();

        if (_targetMeshInstance == null)
        {
            GD.PrintErr("MeshInstance3D not found for BodyPartType: " + meshReplacerOptButton.BodyPartType);
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
        if (GlobalUtil.GetAllNodesByType<BodyPartMeshInstance3D>(_characterModelObject).FirstOrDefault() != null)
        {
            return true;
        }

        return false;
    }

    //Change the Mesh base based on the MeshReplacaeOpt button selection (Connected signal)



    public void OnSaveData(SaveGameData newSaveGameData)
    {
        GD.PrintT("Started OnSaveData from:", this.Name);
        //nodeSaveData.Add(_spriteResolution);
        newSaveGameData.SpriteResolution = _spriteResolution;
        newSaveGameData.SpriteResolutionOptBtn = SpriteSizeOptionButton.Selected;
        newSaveGameData.FrameSkipStep = frameSkipStep;
        //newSaveGameData.ShowPixelEffect = _pixelEffectCheckBtn.ButtonPressed;
        newSaveGameData.PixelEffectLevel = PixelationLevelOptionBtn.Selected;
        newSaveGameData.PlaybackSpeed = _animationPlaybackSpeed;
        newSaveGameData.ShowGrid = _showGridCheckButton.ButtonPressed;
    }

    public void OnLoadData(SaveGameData newLoadData)
    {
        GD.PrintT("Started OnLoadData from:", this.Name);
        _spriteResolution = newLoadData.SpriteResolution;
        SpriteSizeOptionButton.Selected = newLoadData.SpriteResolutionOptBtn;
        frameSkipStep = newLoadData.FrameSkipStep;
        _frameStepTextEdit.Text = frameSkipStep.ToString();
        //_pixelEffectCheckBtn.ButtonPressed = newLoadData.ShowPixelEffect;
        PixelationLevelOptionBtn.Selected = newLoadData.PixelEffectLevel;
        _animationPlaybackSpeed = newLoadData.PlaybackSpeed;
        _playBackSpeedLineEdit.Text = _animationPlaybackSpeed.ToString();
        _showGridCheckButton.ButtonPressed = newLoadData.ShowGrid;

        UpdateViewPorts();
        OnSpriteSizeChanged(newLoadData.SpriteResolutionOptBtn);
        OnPixelationLevelChanged(newLoadData.PixelEffectLevel);
        OnPlayBackSpeedChanged(newLoadData.PlaybackSpeed.ToString());
        OnFrameStepChanged(newLoadData.FrameSkipStep.ToString());
        OnShowGridCheckButtonPressed();

        //EffectsChoicesOptionBtn.Selected = newLoadData.EffectsChoicesOptionBtn;
        OnEffectsChoiceItemSelected(EffectsChoicesOptionBtn.Selected);
        //OnPixelEffectPressed();

        //_settingsMainPanel.Visible = false;
    }
}
