using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public partial class SpriteGenerator : Node
{
    [Export] public Button _startGenerationBtn;
    [Export] public Node3D MainScene3D;
    [Export] public SubViewport _rawViewport;
    [Export] public SubViewportContainer _rawViewportContainer;
    [Export] public SubViewport BgRemoverViewport;
    [Export] public SubViewportContainer BgRemoverViewportContainer;
    [Export] public MeshInstance3D MeshShaderPixel3D;
    [Export] public int frameSkipStep = 4; // Control how frequently frames are captured
    [Export] public bool _clearFolderBeforeGeneration = true;
    [Export(PropertyHint.Range, "1,4,1")] private float _animationPlaybackSpeed = 1.0f;
    [Export] public OptionButton _resolutionOptionBtn;
    [Export] public OptionButton EffectsChoicesOptionBtn;
    [Export] public OptionButton EffectLevelOptionBtn;
    [Export] public HSlider Outline3DStrenghtSlider;
    [Export] public ColorPickerButton Outline3DColorPicker;
    [Export] public LineEdit _frameStepTextEdit;
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
    [Export] private ImgColorReductionTextRect _imgColorReductionTextRect;


    private Node3D _modelPivotNode;
    private Node3D _characterModelObject;
    private Camera3D _camera;
    private AnimationPlayer _animationPlayer;

    public static int _spriteResolution = 256;
    private readonly int[] allAngles = { 0, 45, 90, 135, 180, 225, 270, 315 };
    private int renderAngle = 0;
    private string currentAnimation;
    private string currentAnimationName;
    private int frameIndex;
    private int spriteCount = 1;
    private int spriteSheetCollumnCount = 8;
    private string saveFolder = "Model";


    public override void _Ready()
    {
        _startGenerationBtn.Pressed += OnStartGeneration;
        _resolutionOptionBtn.ItemSelected += OnRenderResolutionChanged;
        EffectLevelOptionBtn.ItemSelected += OnEffectLevelChanged;
        EffectsChoicesOptionBtn.ItemSelected += OnEffectsChoiceItemSelected;
        Outline3DStrenghtSlider.ValueChanged += OnOutline3DStrenghtChanged;
        Outline3DColorPicker.ColorChanged += OnOutline3DColorChanged;
        _frameStepTextEdit.TextChanged += OnFrameStepChanged;
        _playBackSpeedLineEdit.TextChanged += OnPlayBackSpeedChanged;
        _clearFolderCheckBtn.Pressed += OnClearFolderPressed;
        _loadAllAnimationsBtn.Pressed += OnLoadAllAnimationsPressed;
        _showGridCheckButton.Pressed += OnShowGridCheckButtonPressed;
        //MeshReplacer Signals
        _hairColorBtn.ColorChanged += OnHairColorChanged;
        _hairMeshOptBtn.ItemSelected += OnHairMeshOptBtnItemSelected;
        WeaponItemMeshOptBtn.ItemSelected += OnWeaponItemMeshOptBtnItemSelected;

        //Set Default UI Control Values
        _clearFolderCheckBtn.ButtonPressed = _clearFolderBeforeGeneration;
        _frameStepTextEdit.Text = frameSkipStep.ToString();
        _playBackSpeedLineEdit.Text = _animationPlaybackSpeed.ToString();
        _angleSelectionItemList.CreateItemsFromList(allAngles.Select(x => x.ToString()).ToArray());

        //Effects and Core Grid Options
        _showGridCheckButton.ButtonPressed = false;
        _showGridCheckButton.Text = _showGridCheckButton.ButtonPressed.ToString();

        //Set Default Resolution and Shader Strenght
        _resolutionOptionBtn.Selected = _resolutionOptionBtn.ItemCount - 1; //Select the Last Option
        OnRenderResolutionChanged(_resolutionOptionBtn.Selected);
        EffectLevelOptionBtn.Selected = EffectLevelOptionBtn.ItemCount - 1; //Last option 
        OnEffectLevelChanged(EffectLevelOptionBtn.Selected);

        EffectsChoicesOptionBtn.Selected = 0;
        OnEffectsChoiceItemSelected(EffectsChoicesOptionBtn.Selected);
        MeshShaderPixel3D.Visible = true;
        OnEffectLevelChanged(99);
        EffectLevelOptionBtn.Visible = false;
        Outline3DStrenghtSlider.Value = 0.0f;

        //Pass the objects from MainScene3D to the SpriteGenerator
        if (MainScene3D != null)
        {
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

        //Mesh Replace Variables and UI
        MeshReplacer.UpdateUIOptionsSceneItemList(_hairMeshOptBtn, Const.HAIR_SCENES_FOLDER_PATH);
        MeshReplacer.UpdateUIOptionsSceneItemList(WeaponItemMeshOptBtn, Const.WEAPON_SCENES_FOLDER_PATH);
        LoadAllMeshReplacerBtnAndMeshItemData();
        _hairMeshOptBtn.Selected = 0;
        OnHairMeshOptBtnItemSelected(0);
        _hairColorBtn.Color = Colors.White;

        GlobalUtil.SaveFolderPath = ProjectSettings.GlobalizePath(Const.SAVE_GAME_PATH);
        //Update initial views
        UpdateViewPorts();
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
                        //OLDCODE
                        //await SaveFrameAsImgPNG(angle);
                        //Replace with Method to add to a Queue
                        await AddFrameAsImgToQueue(angle);
                    }

                    currentTime += frameInterval;

                    currentFrame++;
                }
            }

            _modelPivotNode.RotationDegrees = new Vector3(0, 0, 0);
        }

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        GenerateSpriteSheet(saveFolder, currentAnimationName + "_spriteSheet", spriteSheetCollumnCount);

    }

    private void AddOriginalImgToQueue(Image img)
    {

    }

    private async Task AddFrameAsImgToQueue(int angle)
    {
        //Get the Frame and Path and File names
        string currentAnimPosInSec = ((float)_animationPlayer.CurrentAnimationPosition).ToString("0.000");
        string path = $"{saveFolder}/{currentAnimationName}_{"angle_" + angle}_{spriteCount}.png";

        GD.PrintT("Adding to Queue : ", spriteCount + "Frame: " + frameIndex + " / FullPath: " + path);

        //Get the Image from the given Frame / From the ViewPort
        Image img = BgRemoverViewport.GetTexture().GetImage();
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw); //Make sure image is updated from Shader





        /////////////////NEW CODE START - #TODO:Generate separate function / Refacot /////////////

        //TODO: Apply Logic to restrict Image Colors to 256 (Max Paltte Size)
        //1. Add each image to a list of images (don't save them yet)
        //2.Run a Task.run to process them as a Queue and them transform their colors.
        // 3.Wait for the Transform effect to finish via a signal
        // 4.Save them one by one listening to the effect signal finished.
        // 5. Method needs to return a new Image


        //Add Image to PrrocessingQueue
        ImageSaver.Instance.AddImgToQueue(ProjectSettings.GlobalizePath(path), img);

        //The Save Logic will now all be handled by the ImageSaver


        /////////////////NEW CODE END/////////////

        //img.FlipY();4


        //img.SavePng(ProjectSettings.GlobalizePath(path));

        spriteCount++;
        frameIndex++;

    }

    private Image GetImgWithColorReduction(Image img, SubViewport viewPort)
    {

        return null;
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
                _spriteResolution = 384;
                break;
            case 4:
                _spriteResolution = 512;
                break;
        }
        GD.PrintT("Sprite Size/Res: " + _spriteResolution);

        UpdateViewPorts();
    }


    private void OnEffectsChoiceItemSelected(long itemSelectedIndex)
    {

        GD.PrintT("Effect Selected: " + itemSelectedIndex);
        switch (itemSelectedIndex)
        {
            case 0:
                //No Effect - Turn off MeshInstance3d
                MeshShaderPixel3D.Visible = true;
                OnEffectLevelChanged(99);
                EffectLevelOptionBtn.Visible = false;
                break;
            case 1:
                //Pixel Effect
                MeshShaderPixel3D.Visible = true;
                EffectLevelOptionBtn.Visible = true;
                OnEffectLevelChanged(EffectLevelOptionBtn.Selected);
                break;
            case 2:
                //Toon Effect
                break;
        }
        UpdateViewPorts();
    }

    private void OnEffectLevelChanged(long itemSelectedIndex)
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
                shaderResolution = 128;
                break;
            case 4:
                shaderResolution = 192;
                break;
            case 5:
                shaderResolution = 256;
                break;
            case 99:
                shaderResolution = 512;
                break;

        }

        GD.PrintT("Effect Pixel Resolution: " + _spriteResolution);

        if (MeshShaderPixel3D.Mesh.SurfaceGetMaterial(0) is ShaderMaterial shaderMaterial)
        {
            shaderMaterial.SetShaderParameter("target_resolution", shaderResolution);
        }
        UpdateViewPorts();
    }


    private void OnOutline3DStrenghtChanged(double value)
    {
        if (MeshShaderPixel3D.Mesh.SurfaceGetMaterial(0) is ShaderMaterial shaderMaterial)
        {
            shaderMaterial.SetShaderParameter("outline_strength", value);
        }
    }

    private void OnOutline3DColorChanged(Color color)
    {
        if (MeshShaderPixel3D.Mesh.SurfaceGetMaterial(0) is ShaderMaterial shaderMaterial)
        {
            shaderMaterial.SetShaderParameter("outline_color", color);
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
        //newSaveGameData.ShowPixelEffect = _pixelEffectCheckBtn.ButtonPressed;
        newSaveGameData.PixelEffectLevel = EffectLevelOptionBtn.Selected;
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
        //_pixelEffectCheckBtn.ButtonPressed = newLoadData.ShowPixelEffect;
        //_pixelEffectCheckBtn.Text = _pixelEffectCheckBtn.ButtonPressed.ToString();
        EffectLevelOptionBtn.Selected = newLoadData.PixelEffectLevel;
        _animationPlaybackSpeed = newLoadData.PlaybackSpeed;
        _playBackSpeedLineEdit.Text = _animationPlaybackSpeed.ToString();
        _showGridCheckButton.ButtonPressed = newLoadData.ShowGrid;

        UpdateViewPorts();
        OnRenderResolutionChanged(newLoadData.SpriteResolutionOptBtn);
        OnEffectLevelChanged(newLoadData.PixelEffectLevel);
        OnPlayBackSpeedChanged(newLoadData.PlaybackSpeed.ToString());
        OnFrameStepChanged(newLoadData.FrameSkipStep.ToString());
        OnShowGridCheckButtonPressed();

        //EffectsChoicesOptionBtn.Selected = newLoadData.EffectsChoicesOptionBtn;
        OnEffectsChoiceItemSelected(EffectsChoicesOptionBtn.Selected);
        //OnPixelEffectPressed();

        //_settingsMainPanel.Visible = false;
    }
}
