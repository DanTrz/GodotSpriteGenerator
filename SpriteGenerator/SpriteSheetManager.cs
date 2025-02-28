using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Image = Godot.Image;

public partial class SpriteSheetManager : PanelContainer
{
    [Export] private TextureRect _textureRect;
    [Export] private HSlider _saturationSlider;
    [Export] private Label _saturationLabel;
    [Export] private Label _brightnessLabel;
    [Export] private Label _outlineLabel;
    [Export] private HSlider _brightnessSlider;
    [Export] private CheckBox _outlineCheckbox;
    [Export] private HSlider _outlineThicknessSlider;
    [Export] private ColorPickerButton _outlineColorPicker;
    [Export] private CheckBox _ditheringCheckbox;
    [Export] private HSlider _ditheringSlider;
    [Export] private CheckBox _colorReductionCheckbox;
    [Export] private SpinBox _colorCountSpinBox;
    [Export] private Button _saveButton;
    [Export] private Label _statusLabel;

    // Add toggles for enabling/disabling shader effects
    [Export] private CheckBox _enableSaturationCheckbox;
    [Export] private CheckBox _enableBrightnessCheckbox;

    private Texture2D _originalTexture;
    private Image _originalImage;

    private bool _isProcessing;
    public bool IsEffectProcessing
    {
        get
        {
            return _isProcessing;
        }
        set
        {
            _isProcessing = value;
            if (_isProcessing)
            {
                ProcessMode = ProcessModeEnum.Disabled;
            }
            else
            {
                ProcessMode = ProcessModeEnum.Always;
            }
        }
    }
    private ShaderMaterial _shaderMaterial; // Store the ShaderMaterial

    public override void _Ready()
    {
        if (_textureRect.Texture != null)
        {
            _originalTexture = _textureRect.Texture;
            _originalImage = _originalTexture.GetImage();
        }

        // Get a reference to the ShaderMaterial ---
        _shaderMaterial = (ShaderMaterial)_textureRect.Material;
        if (_shaderMaterial == null)
        {
            GD.PrintErr("TextureRect does not have a ShaderMaterial assigned in the editor!");
            return; // Exit if no ShaderMaterial is found to avoid errors
        }

        //Connect UI Sigansl for most of the effects
        // with this Syntaxe - I do not pass the value parameter to QueueApplyEffects().
        _saturationSlider.ValueChanged += (value) => QueueApplyEffects();
        _brightnessSlider.ValueChanged += (value) => QueueApplyEffects();
        _outlineCheckbox.Toggled += (pressed) => QueueApplyEffects();
        _outlineThicknessSlider.ValueChanged += (value) => QueueApplyEffects();
        _outlineColorPicker.ColorChanged += (color) => QueueApplyEffects();
        _ditheringCheckbox.Toggled += (pressed) => QueueApplyEffects();
        _ditheringSlider.ValueChanged += (value) => QueueApplyEffects();
        //_colorReductionCheckbox.Toggled += (pressed) => QueueApplyEffects();
        //_colorCountSpinBox.ValueChanged += (value) => QueueApplyEffects();
        _saveButton.Pressed += OnSaveButtonPressed;

        // Connect Shader Related Effects for Brightness, Color Reduction and Saturation
        _enableSaturationCheckbox.Toggled += (pressed) => UpdateBrightnessAndSaturationShader();
        _enableBrightnessCheckbox.Toggled += (pressed) => UpdateBrightnessAndSaturationShader();
        _colorCountSpinBox.ValueChanged += (value) => UpdateBrightnessAndSaturationShader();
        _colorReductionCheckbox.Toggled += (pressed) => UpdateBrightnessAndSaturationShader();

        _statusLabel.Text = "Ready";

        UpdateUIElementsOnLoad();
        QueueApplyEffects();
        UpdateBrightnessAndSaturationShader(); // Initial shader parameter update

    }

    private void UpdateUIElementsOnLoad()
    {
        _brightnessSlider.Value = 1.0f;
        _enableBrightnessCheckbox.ButtonPressed = false;
        _saturationSlider.Value = 1.0f;
        _saturationLabel.Text = _saturationSlider.Value.ToString("0.0");
        _brightnessLabel.Text = _brightnessSlider.Value.ToString("0.0");
        _outlineLabel.Text = _outlineThicknessSlider.Value.ToString("0.0");
        _enableSaturationCheckbox.ButtonPressed = false;
        _outlineCheckbox.ButtonPressed = false;
        _outlineThicknessSlider.Value = 0;
        _outlineColorPicker.Color = Colors.Black;
        _ditheringCheckbox.ButtonPressed = false;
        _ditheringSlider.Value = 0.0f;
        _colorReductionCheckbox.ButtonPressed = false;
        _colorCountSpinBox.Value = GlobalUtil.GetTotalColorCount(_originalImage);
        _colorCountSpinBox.MaxValue = GlobalUtil.GetTotalColorCount(_originalImage);
    }

    private void QueueApplyEffects()
    {
        //Update UI Elements where needed:
        _outlineLabel.Text = _outlineThicknessSlider.Value.ToString("0.0");

        // Check if processing is already in progress
        if (IsEffectProcessing) return;
        IsEffectProcessing = true;

        // Capture UI state for image processing effects (excluding shader controls
        // This is needed as I don't receive the values from the Signal Connection
        // This is due to choice I made to connect all to the same Method QueueApplyEffects
        bool doColorReduction = _colorReductionCheckbox?.ButtonPressed ?? false;
        int colorCount = doColorReduction ? (int)(_colorCountSpinBox?.Value ?? 0) : 0;
        bool doOutline = _outlineCheckbox?.ButtonPressed ?? false;
        int outlineThickness = doOutline ? (int)(_outlineThicknessSlider?.Value ?? 0) : 0;
        Color outlineColor = doOutline ? (_outlineColorPicker?.Color ?? Colors.Black) : Colors.Black;
        bool doDithering = _ditheringCheckbox?.ButtonPressed ?? false;
        float ditheringStrength = doDithering ? (float)(_ditheringSlider?.Value ?? 0.0f) : 0.0f;

        // Apply effects in a separate thread in parallel (Forced via Task.Run)
        Task.Run(() => ApplyEffectsAsync(doColorReduction, colorCount, doOutline, outlineThickness,
            outlineColor, doDithering, ditheringStrength));
    }

    private async Task ApplyEffectsAsync(bool doColorReduction, int colorCount, bool doOutline,
        int outlineThickness, Color outlineColor, bool doDithering, float ditheringStrength)
    {

        //I always start with a copy of the original image, preserving the original for "undo" (to be implemented)
        Image modifiedImage = (Image)_originalImage.Duplicate(); //First code version (working)
        //Image modifiedImage = (Image)_textureRect.Texture.GetImage();
        //Image modifiedImage = (Image)_textureRect.Texture.GetImage().Duplicate();//TODO: Find a way to store the last image reduced color without other effects, to use as a enw starting point

        // Apply All effects in sequence, to ensure they are applied in the correct order
        // Even if only one is modified, I update all to avoid conflicts. 
        //if (doColorReduction)
        //{
        //    //if (colorCount != (int)GetTotalColorCount(modifiedImage))
        //    //{
        //    CallDeferred(nameof(SetStatus), "Reducing colors...");
        //    modifiedImage = await Task.Run(() => ReduceColors(modifiedImage, colorCount));
        //    CallDeferred(nameof(SetStatus), "Color reduction complete.");
        //    //}
        //}

        if (doOutline)
        {
            CallDeferred(nameof(SetStatus), "Applying outline...");
            modifiedImage = await Task.Run(() => AddOutline(modifiedImage, outlineThickness, outlineColor));
            CallDeferred(nameof(SetStatus), "Outline complete.");
        }

        if (doDithering)
        {
            CallDeferred(nameof(SetStatus), "Applying dithering...");
            modifiedImage = await Task.Run(() => ApplyDithering(modifiedImage, ditheringStrength));
            CallDeferred(nameof(SetStatus), "Dithering complete.");
        }

        // Update the texture *after* other effects have been applied
        CallDeferred(nameof(UpdateTexture), modifiedImage);
    }

    private void UpdateBrightnessAndSaturationShader()
    {
        if (_shaderMaterial == null) return; // Safety check

        // Set shader parameters based on UI controls
        _shaderMaterial.SetShaderParameter("enable_saturation", _enableSaturationCheckbox.ButtonPressed);
        _shaderMaterial.SetShaderParameter("saturation", _saturationSlider.Value);
        _shaderMaterial.SetShaderParameter("enable_brightness", _enableBrightnessCheckbox.ButtonPressed);
        _shaderMaterial.SetShaderParameter("brightness", _brightnessSlider.Value);
        _shaderMaterial.SetShaderParameter("enable_color_reduction", _colorReductionCheckbox.ButtonPressed);
        _shaderMaterial.SetShaderParameter("num_colors", _colorCountSpinBox.Value);

        // Update UI labels with current values
        _saturationLabel.Text = _saturationSlider.Value.ToString("0.00");
        _brightnessLabel.Text = _brightnessSlider.Value.ToString("0.00");
    }

    private Image AddOutline(Image image, int thickness, Color color)
    {
        int width = image.GetWidth();
        int height = image.GetHeight();
        byte[] originalData = image.GetData();
        byte[] outlinedData = new byte[originalData.Length];
        Array.Copy(originalData, outlinedData, originalData.Length);

        byte outlineR = (byte)(color.R * 255);
        byte outlineG = (byte)(color.G * 255);
        byte outlineB = (byte)(color.B * 255);
        byte outlineA = (byte)(color.A * 255);

        Parallel.For(0, height, y =>
        {
            for (int x = 0; x < width; x++)
            {
                int originalIndex = (y * width + x) * 4;
                if (originalData[originalIndex + 3] == 0) continue;
                for (int oy = Math.Max(0, y - thickness); oy <= Math.Min(height - 1, y + thickness); oy++)
                {
                    for (int ox = Math.Max(0, x - thickness); ox <= Math.Min(width - 1, x + thickness); ox++)
                    {
                        if (ox == x && oy == y) continue;
                        int outlinedIndex = (oy * width + ox) * 4;
                        if (outlinedData[outlinedIndex + 3] == 0)
                        {
                            outlinedData[outlinedIndex + 0] = outlineR;
                            outlinedData[outlinedIndex + 1] = outlineG;
                            outlinedData[outlinedIndex + 2] = outlineB;
                            outlinedData[outlinedIndex + 3] = outlineA;
                        }
                    }
                }
            }
        });

        return Image.CreateFromData(width, height, false, Image.Format.Rgba8, outlinedData);
    }

    //TODO: CURRENTLY NOT WORKING - #BUG TO FIX. 
    private Image ApplyDithering(Image image, float strength)
    {
        int width = image.GetWidth();
        int height = image.GetHeight();
        byte[] data = image.GetData(); // Get data directly

        // Floyd-Steinberg dithering
        Parallel.For(0, height, y =>
        {
            // Local error buffer, one float per channel (R, G, B, A) per pixel.
            float[] errorBufferR = new float[width];
            float[] errorBufferG = new float[width];
            float[] errorBufferB = new float[width];
            //float[] errorBufferA = new float[width]; // Alpha is not dithered

            for (int x = 0; x < width; x++)
            {
                int index = (y * width + x) * 4;

                // Apply error from the error buffer
                float oldR = GlobalUtil.ClampFloat(data[index] + errorBufferR[x], 0, 255);
                float oldG = GlobalUtil.ClampFloat(data[index + 1] + errorBufferG[x], 0, 255);
                float oldB = GlobalUtil.ClampFloat(data[index + 2] + errorBufferB[x], 0, 255);
                float oldA = data[index + 3]; // Alpha is unchanged
                Color oldPixel = new Color(oldR / 255f, oldG / 255f, oldB / 255f, oldA / 255f);

                Color newPixel = GlobalUtil.FindClosestPaletteColor(oldPixel);

                // Directly modify the data array (in-place modification)
                data[index] = (byte)GlobalUtil.ClampFloat(newPixel.R * 255, 0, 255);
                data[index + 1] = (byte)GlobalUtil.ClampFloat(newPixel.G * 255, 0, 255);
                data[index + 2] = (byte)GlobalUtil.ClampFloat(newPixel.B * 255, 0, 255);
                data[index + 3] = (byte)GlobalUtil.ClampFloat(newPixel.A * 255, 0, 255); // Keep original alpha


                // Calculate error *per channel*
                float errorR = oldR - (newPixel.R * 255f);
                float errorG = oldG - (newPixel.G * 255f);
                float errorB = oldB - (newPixel.B * 255f);
                //float errorA = oldA - (newPixel.A * 255f); // No alpha error

                // Distribute error
                if (x + 1 < width)
                {
                    errorBufferR[x + 1] += errorR * (7.0f / 16.0f) * strength;
                    errorBufferG[x + 1] += errorG * (7.0f / 16.0f) * strength;
                    errorBufferB[x + 1] += errorB * (7.0f / 16.0f) * strength;
                }
                if (y + 1 < height)
                {
                    if (x - 1 >= 0)
                    {
                        int nextIndex = ((y + 1) * width + (x - 1)) * 4;
                        data[nextIndex] = (byte)GlobalUtil.ClampFloat(data[nextIndex] + errorR * (3.0f / 16.0f) * strength, 0, 255);
                        data[nextIndex + 1] = (byte)GlobalUtil.ClampFloat(data[nextIndex + 1] + errorG * (3.0f / 16.0f) * strength, 0, 255);
                        data[nextIndex + 2] = (byte)GlobalUtil.ClampFloat(data[nextIndex + 2] + errorB * (3.0f / 16.0f) * strength, 0, 255);
                    }
                    if (x + 1 < width)
                    {
                        int nextIndex = ((y + 1) * width + (x + 1)) * 4;
                        data[nextIndex] = (byte)GlobalUtil.ClampFloat(data[nextIndex] + errorR * (1.0f / 16.0f) * strength, 0, 255);
                        data[nextIndex + 1] = (byte)GlobalUtil.ClampFloat(data[nextIndex + 1] + errorG * (1.0f / 16.0f) * strength, 0, 255);
                        data[nextIndex + 2] = (byte)GlobalUtil.ClampFloat(data[nextIndex + 2] + errorB * (1.0f / 16.0f) * strength, 0, 255);
                    }
                    int nextIndexCenter = ((y + 1) * width + x) * 4;
                    data[nextIndexCenter] = (byte)GlobalUtil.ClampFloat(data[nextIndexCenter] + errorR * (5.0f / 16.0f) * strength, 0, 255);
                    data[nextIndexCenter + 1] = (byte)GlobalUtil.ClampFloat(data[nextIndexCenter + 1] + errorG * (5.0f / 16.0f) * strength, 0, 255);
                    data[nextIndexCenter + 2] = (byte)GlobalUtil.ClampFloat(data[nextIndexCenter + 2] + errorB * (5.0f / 16.0f) * strength, 0, 255);
                }
            }
        });

        return Image.CreateFromData(width, height, false, Image.Format.Rgba8, data);
    }

    private Image ReduceColors(Image image, int numColors)
    {
        List<Color> palette = GlobalUtil.KMeansClustering(image, numColors, _colorCountSpinBox);
        int width = image.GetWidth();
        int height = image.GetHeight();
        byte[] originalData = image.GetData();
        byte[] reducedData = new byte[originalData.Length];

        Parallel.For(0, height, y =>
        {
            for (int x = 0; x < width; x++)
            {
                int index = (y * width + x) * 4;
                Color originalColor = GlobalUtil.GetColorFromBytes(originalData, index);

                if (originalColor.A == 0)
                {
                    Array.Copy(originalData, index, reducedData, index, 4);
                }
                else
                {
                    Color closestColor = GlobalUtil.FindClosestColor(originalColor, palette);
                    closestColor.A = originalColor.A; // Preserve Alpha
                    GlobalUtil.SetColorToBytes(reducedData, index, closestColor);
                }
            }
        });

        return Image.CreateFromData(width, height, false, Image.Format.Rgba8, reducedData);
    }

    //Update UI Label with current status of effects processing
    private void SetStatus(string text)
    {
        _statusLabel.Text = text;
        GD.Print(text);
    }

    //Save new Image to folder
    private void OnSaveButtonPressed()
    {
        if (_textureRect.Texture == null) return;
        Image modifiedImage = ((ImageTexture)_textureRect.Texture).GetImage();

        using FileDialog fileDialog = new FileDialog
        {
            FileMode = FileDialog.FileModeEnum.SaveFile,
            Filters = new string[] { "*.png ; PNG Images" },
            Access = FileDialog.AccessEnum.Filesystem
        };

        AddChild(fileDialog); // Add to scene first 

        string folderCurrentDir = GlobalUtil.SaveFolderPath; // Ensure it's inside user:// or res://
        string globalizedPath = ProjectSettings.GlobalizePath(folderCurrentDir);

        if (!GlobalUtil.HasDirectory(globalizedPath, this))
        {
            GD.Print("Directory does NOT exist: " + folderCurrentDir);
            globalizedPath = "res://"; // Fallback to a safe default
        }

        fileDialog.CurrentDir = globalizedPath; //Set Current Directory at the end after adding Child to Scene otherwise it was not working

        fileDialog.FileSelected += (path) => SaveImageToFile(modifiedImage, path);

        fileDialog.PopupCentered(); // Show the dialog

    }


    private void OnFileSelected(string path)
    {
        GD.Print("Selected file path: " + path); //handle file selection
    }

    private void SaveImageToFile(Image image, string filePath)
    {
        Error err = image.SavePng(filePath);
        if (err != Error.Ok) GD.PrintErr("Error saving image: " + err);
    }

    //Method used to update the TextureRect with the modified image
    private void UpdateTexture(Image modifiedImage)
    {
        _textureRect.Texture = ImageTexture.CreateFromImage(modifiedImage);

        //-> Update shader params after image update with other C# code effects.
        UpdateBrightnessAndSaturationShader(); // Shader changes always last to ensure we will not get mixed up with other effects

        IsEffectProcessing = false;
        SetStatus("Ready");

    }




}
