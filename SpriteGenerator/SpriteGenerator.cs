using Godot;
using System;
using System.IO;

public partial class SpriteGenerator : Node
{
    [Export] private Button _startGenerationBtn;

    [Export] private MainScene3d _MainScene3D;
    [Export] private SubViewport _viewport;
    [Export] private SubViewportContainer _viewportContainer;
    [Export] private string _outputFolder = "res://SpriteSheets";
    [Export] private int _spriteResolution = 256;
    [Export] private int frameSkipStep = 4; // Control how frequently frames are captured

    private OptionButton _resolutionOptionBtn;

    private Node3D _model;
    private Camera3D _camera;
    private AnimationPlayer _animationPlayer;

    private readonly int[] angles = { 0 };
    //private readonly int[] angles = { 0, 45, 90, 135, 180, 225, 270, 315 };
    private string currentAnimation;
    private int frameIndex;
    private int spriteCount = 1;
    private string saveFolder = "Model";

    public override void _Ready()
    {
        _startGenerationBtn.Pressed += OnStartGeneration;
        _resolutionOptionBtn = GetNode<OptionButton>("%ResOptionButton");
        _resolutionOptionBtn.ItemSelected += OnRenderResolutionChanged;

        //Pass the objects from MainScene3D to the SpriteGenerator
        if (_MainScene3D != null)
        {
            _model = _MainScene3D.MainModel;
            _camera = _MainScene3D.MainCamera;
            _animationPlayer = _MainScene3D.MainAnimationPlayer;
        }
        else
        {
            GD.PrintErr("MainScene3D is null");
        }

        UpdateViewPort();


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

    private void UpdateViewPort()
    {
        Vector2I viewPortSize = new Vector2I(_spriteResolution, _spriteResolution);
        _viewport.CallDeferred("set_size", viewPortSize);
        _viewportContainer.CallDeferred("set_size", viewPortSize);

        //_viewport.Size = viewPortSize;
        //_viewportContainer.Size = viewPortSize;

    }

    private void OnStartGeneration()
    {
        saveFolder = ProjectSettings.GlobalizePath(_outputFolder + "/" + _model.Name);

        if (!Directory.Exists(ProjectSettings.GlobalizePath(saveFolder)))
            Directory.CreateDirectory(ProjectSettings.GlobalizePath(saveFolder));

        GD.PrintT("Start Generation");

        GenerateSprites();
    }

    private async void GenerateSprites()
    {
        foreach (var anim in _animationPlayer.GetAnimationList())
        {
            if (anim == "RESET") continue;

            currentAnimation = anim;
            _animationPlayer.Play(anim);
            frameIndex = 0;

            while (_animationPlayer.IsPlaying())
            {
                foreach (var angle in angles)
                {
                    _model.RotationDegrees = new Vector3(0, angle, 0);

                    //Only capture frames where frameIndex is a multiple of frameStep
                    if (frameIndex % frameSkipStep == 0)
                    {
                        await ToSignal(GetTree(), "process_frame");
                        SaveFrame(angle);
                    }
                    frameIndex++;
                }
                await ToSignal(GetTree(), "process_frame");
            }
        }

        GenerateSpriteSheet(saveFolder, currentAnimation + "_spriteSheet", 4);
    }

    private void SaveFrame(int angle)
    {
        var img = _viewport.GetTexture().GetImage();

        //img.FlipY();4
        string path = $"{saveFolder}/{currentAnimation}_{"angle_" + angle}_{spriteCount}.png";
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
    }



}
