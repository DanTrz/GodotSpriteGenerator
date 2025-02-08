using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using Godot;

public partial class SpriteGenerator : Node
{
    [Export] private Button _startGenerationBtn;

    [Export] public MainScene3d MainScene3D;
    [Export] private SubViewport _rawViewport;
    [Export] private SubViewportContainer _rawViewportContainer;
    [Export] private string _outputFolder = "res://SpriteSheets";
    [Export] private int _spriteResolution = 256;
    [Export] private int frameSkipStep = 4; // Control how frequently frames are captured
    [Export] private bool _clearFolderBeforeGeneration = true;
    [Export] private bool _usePixelEffect = true;
    [Export] private bool _isTimeBaseExport = true;
    [Export(PropertyHint.Range, "1,4,1")] private float _animationPlaybackSpeed = 1.0f;

    [OnReady("%SaveIntervalTimer")] private Timer _saveIntervalTimer;
    [OnReady("%ResOptionButton")] private OptionButton _resolutionOptionBtn;
    [OnReady("%PixelShaderOptionBtn")] private OptionButton _pixelShaderOptionBtn;
    [OnReady("%FrameStepTextEdit")] private LineEdit _frameStepTextEdit;
    [OnReady("%PlayBackSpeedLineEdit")] private LineEdit _playBackSpeedLineEdit;
    [OnReady("%ClearFolderCheckBtn")] private CheckButton _clearFolderCheckBtn;
    [OnReady("%PixelEffectCheckBtn")] private CheckButton _pixelEffectCheckBtn;
    [OnReady("%PixelShaderMesh")] private MeshInstance3D _pixelShaderMesh;
    [OnReady("%3DModelPositionManager")] private ModelPositionManager _modelPositionManager;
    [OnReady("%LoadAllAnimationsBtn")] private Button _loadAllAnimationsBtn;
    [OnReady("%animSelectionItemList")] private ItemListCheckBox _animSelectionItemList;
    [OnReady("%angleSelectionItemList")] private ItemListCheckBox _angleSelectionItemList;

    private Node3D _model;
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


    public override void _Ready()
    {
        //Set Default UI Control Values
        _clearFolderCheckBtn.ButtonPressed = _clearFolderBeforeGeneration;
        _pixelEffectCheckBtn.ButtonPressed = _usePixelEffect;
        _pixelShaderMesh.Visible = _usePixelEffect;
        _frameStepTextEdit.Text = frameSkipStep.ToString();
        _playBackSpeedLineEdit.Text = _animationPlaybackSpeed.ToString();
        _angleSelectionItemList.CreateItemsFromList(allAngles.Select(x => x.ToString()).ToArray());


        //Set Default Resolution and Shader Strenght
        _resolutionOptionBtn.Selected = _resolutionOptionBtn.ItemCount - 1;
        OnRenderResolutionChanged(_resolutionOptionBtn.ItemCount - 1);
        _pixelShaderOptionBtn.Selected = _pixelShaderOptionBtn.ItemCount - 1;
        OnPixelShaderResolutionChanged(_pixelShaderOptionBtn.ItemCount - 1);

        //Connect Signals
        _startGenerationBtn.Pressed += OnStartGeneration;
        _resolutionOptionBtn.ItemSelected += OnRenderResolutionChanged;
        _pixelShaderOptionBtn.ItemSelected += OnPixelShaderResolutionChanged;
        _frameStepTextEdit.TextChanged += OnFrameStepChanged;
        _playBackSpeedLineEdit.TextChanged += OnPlayBackSpeedChanged;
        _clearFolderCheckBtn.Pressed += OnClearFolderPressed;
        _pixelEffectCheckBtn.Pressed += OnPixelEffectPressed;
        _loadAllAnimationsBtn.Pressed += OnLoadAllAnimationsPressed;
        _saveIntervalTimer.Timeout += OnSaveIntervalTimerTimeout;


        //Pass the objects from MainScene3D to the SpriteGenerator
        if (MainScene3D != null)
        {
            _model = MainScene3D.MainModelNode;
            _camera = MainScene3D.MainCamera;
            _animationPlayer = MainScene3D.MainAnimationPlayer;
            _characterModelObject = MainScene3D.MainCharacterObj;

            //Pass the Model to te PositionManager 
            _modelPositionManager.ModelNode = _model;
            _modelPositionManager.CameraNode = _camera;
        }
        else
        {
            GD.PrintErr("MainScene3D is null");
        }

        _animationPlayer.AnimationFinished += OnAnimationFinished;

        UpdateViewPort();


    }


    public override void _Process(double delta)
    {
        GD.Print("Print Test 4");
        GD.PrintT("PrintT Test");
    }

    private void OnStartGeneration()
    {
        spriteCount = 0;
        saveFolder = ProjectSettings.GlobalizePath(_outputFolder + "/" + _characterModelObject.Name);

        if (!Directory.Exists(ProjectSettings.GlobalizePath(saveFolder)))
            Directory.CreateDirectory(ProjectSettings.GlobalizePath(saveFolder));

        // GD.PrintT("Start Generation");

        if (_clearFolderBeforeGeneration)
            ClearFolder(saveFolder);

        if (_isTimeBaseExport)
        {
            GenerateSpritesTimeBased();
        }
        else
        {
            GenerateSpritesFrameBased2();
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

        _model.RotationDegrees = new Vector3(0, 0, 0);

        // foreach (var anim in _animationPlayer.GetAnimationList())
        foreach (var selectedAnimItem in _animSelectionItemList.GetSelectedItems())
        {
            string anim = _animSelectionItemList.GetItemText(selectedAnimItem);

            if (anim == "RESET" || anim == "TPose") continue;

            currentAnimation = anim;
            currentAnimationName = anim.Replace("/", "_");
            // currentAnimation = anim;
            // currentAnimation = anim.GetBaseName();

            foreach (var angle in selectedAngles)
            {
                _model.RotationDegrees = new Vector3(0, angle, 0);

                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); //Give time to the model to rotate.

                frameIndex = 0;

                _animationPlayer.Play(anim);
                GD.PrintT("FRAME BASED = save every: " + frameSkipStep + " frames");

                while (_animationPlayer.IsPlaying())
                {
                    await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); //Skip one frame.

                    //Only capture frames where frameIndex is a multiple of frameStep
                    if (frameIndex % frameSkipStep == 0)
                    {
                        SaveFrameRaw(angle);
                    }
                    frameIndex++;
                }
            }

            _model.RotationDegrees = new Vector3(0, 0, 0);
        }

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        GenerateSpriteSheet(saveFolder, currentAnimationName + "_spriteSheet", 4);
    }

    private async void GenerateSpritesTimeBased()
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

        _model.RotationDegrees = new Vector3(0, 0, 0);

        // foreach (var anim in _animationPlayer.GetAnimationList())
        foreach (var selectedAnimItem in _animSelectionItemList.GetSelectedItems())
        {
            string anim = _animSelectionItemList.GetItemText(selectedAnimItem);

            if (anim == "RESET" || anim == "TPose") continue;

            currentAnimation = anim;
            currentAnimationName = anim.Replace("/", "_");

            foreach (var angle in selectedAngles)
            {
                spriteCount = 0;

                _model.RotationDegrees = new Vector3(0, angle, 0);

                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); //Give time to the model to rotate.

                float saveFileSecondsInterval = Const.ImgRenderTimeInterval.GetValueOrDefault((int)_animationPlaybackSpeed);
                //float saveFileSecondsInterval = Const.ImgRenderTimeInterval.GetValueOrDefault(4);


                // TODO / #BUG Improve this part - I have not fond a way to consistely reposition the animation on the screen without stopping and playing it again. It's working for nowbut needs a better solution.
                _animationPlayer.Play(anim);
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); //Gives time to position the AnimPlayer.
                _animationPlayer.Stop(); //Stops the animation and reset to make sure the first image will show the first frame. 

                //TODO NEW PIECE OF CODE.....
                float awaitTime = (_animationPlayer.GetAnimation(anim).Step) / _animationPlaybackSpeed;
                awaitTime = 0.001f;

                _saveIntervalTimer.WaitTime = awaitTime;

                renderAngle = angle;

                //Starts playing the animation for capture pngs.  
                _animationPlayer.Play(anim);

                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); //Give time for AnimPlayer to push to first frame. 

                _saveIntervalTimer.Start();

                GD.PrintT("TIME BASED = save every sec: " + awaitTime);

                await ToSignal(_animationPlayer, AnimationPlayer.SignalName.AnimationFinished);

            }
        }
    }

    private async void OnSaveIntervalTimerTimeout()
    {
        SaveFrameRaw(renderAngle);
    }

    private async void OnAnimationFinished(StringName animName)
    {
        if (!_isTimeBaseExport) return;

        _saveIntervalTimer.Stop();
        _model.RotationDegrees = new Vector3(0, 0, 0);

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        GenerateSpriteSheet(saveFolder, currentAnimationName + "_spriteSheet", 4);
    }

    private async void GenerateSpritesFrameBased2()
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

        _model.RotationDegrees = new Vector3(0, 0, 0);

        foreach (var selectedAnimItem in _animSelectionItemList.GetSelectedItems())
        {
            string anim = _animSelectionItemList.GetItemText(selectedAnimItem);

            if (anim == "RESET" || anim == "TPose") continue;

            currentAnimation = anim;
            currentAnimationName = anim.Replace("/", "_");

            foreach (var angle in selectedAngles)
            {
                _model.RotationDegrees = new Vector3(0, angle, 0);

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
                        SaveFrameRaw(angle);
                    }

                    currentTime += frameInterval;

                    currentFrame++;
                }
            }

            _model.RotationDegrees = new Vector3(0, 0, 0);
        }

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        GenerateSpriteSheet(saveFolder, currentAnimationName + "_spriteSheet", 4);

    }

    private void SaveFrameRaw(int angle)
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

    // private void SaveFramePixelShader(int angle)
    // {
    //     GD.PrintT("Saving Sprite with Pixel Shader : ", spriteCount);
    //     var img = _textureRectPixelShader.Texture.GetImage();

    //     string path = $"{saveFolder}/{currentAnimationName}_{"angle_" + angle}_{spriteCount}.png";
    //     spriteCount++;

    //     img.SavePng(ProjectSettings.GlobalizePath(path));
    //     frameIndex++;

    // }

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
                shaderResolution = 148;
                break;
            case 5:
                shaderResolution = 192;
                break;
        }

        ((ShaderMaterial)_pixelShaderMesh.MaterialOverride).SetShaderParameter("pixel_size", shaderResolution);
        GD.PrintT("Shader resolution: " + shaderResolution);

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


    private void OnPixelEffectPressed()
    {
        _usePixelEffect = _pixelEffectCheckBtn.ButtonPressed;
        _pixelShaderMesh.Visible = _usePixelEffect;

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









}
