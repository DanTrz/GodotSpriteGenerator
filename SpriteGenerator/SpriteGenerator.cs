using Godot;
using System;
using System.IO;
using System.Net.Sockets;
using System.Linq;

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

    [OnReady("%ResOptionButton")] private OptionButton _resolutionOptionBtn;
    [OnReady("%PixelShaderOptionBtn")] private OptionButton _pixelShaderOptionBtn;
    [OnReady("%FrameStepTextEdit")] private LineEdit _frameStepTextEdit;
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
        _clearFolderCheckBtn.Pressed += OnClearFolderPressed;
        _pixelEffectCheckBtn.Pressed += OnPixelEffectPressed;
        _loadAllAnimationsBtn.Pressed += OnLoadAllAnimationsPressed;

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

        UpdateViewPort();


    }


    private void OnStartGeneration()
    {
        spriteCount = 1;
        saveFolder = ProjectSettings.GlobalizePath(_outputFolder + "/" + _characterModelObject.Name);

        if (!Directory.Exists(ProjectSettings.GlobalizePath(saveFolder)))
            Directory.CreateDirectory(ProjectSettings.GlobalizePath(saveFolder));

        // GD.PrintT("Start Generation");

        if (_clearFolderBeforeGeneration)
            ClearFolder(saveFolder);

        GenerateSprites();
    }

    private void ClearFolder(string folder)
    {
        string[] files = Directory.GetFiles(folder);
        foreach (string file in files)
        {
            File.Delete(file);
        }
    }

    private async void GenerateSprites()
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

        _animationPlayer.SpeedScale = 2.5f; //This seems the value closer to the blender rendering speed (Gives me the closer match of sprite generation per Frame Skip Step)

        // foreach (var anim in _animationPlayer.GetAnimationList())
        foreach (var selectedAnimItem in _animSelectionItemList.GetSelectedItems())
        {
            string anim = _animSelectionItemList.GetItemText(selectedAnimItem);

            if (anim == "RESET" || anim == "TPose") continue;

            currentAnimation = anim;
            currentAnimationName = anim.Replace("/", "_");
            // currentAnimation = anim;
            // currentAnimation = anim.GetBaseName();
            _animationPlayer.Play(anim);
            frameIndex = 0;

            while (_animationPlayer.IsPlaying())
            {
                foreach (var angle in selectedAngles)
                {
                    _model.RotationDegrees = new Vector3(0, angle, 0);

                    await ToSignal(GetTree().CreateTimer(1.0), Timer.SignalName.Timeout);
                    GD.Print("Pause finshed - Continue Process Sprite generation");

                    //Only capture frames where frameIndex is a multiple of frameStep
                    if (frameIndex % frameSkipStep == 0)
                    {
                        await ToSignal(GetTree(), "process_frame");
                        SaveFrameRaw(angle);

                        // if (_usePixelEffect)
                        // {
                        //     SaveFramePixelShader(angle);
                        // }
                        // else
                        // {
                        //     SaveFrameRaw(angle);
                        // }

                    }
                    frameIndex++;
                }
                await ToSignal(GetTree(), "process_frame");
            }
        }

        GenerateSpriteSheet(saveFolder, currentAnimationName + "_spriteSheet", 4);
    }

    private void SaveFrameRaw(int angle)
    {
        GD.PrintT("Saving Sprite Raw : ", spriteCount);
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
