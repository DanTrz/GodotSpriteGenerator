using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Image = Godot.Image;

public partial class SpriteSheetManager : PanelContainer
    {
    [Export] private TextureRect _textureRect;
    [Export] private HSlider _saturationSlider;
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
    private bool _isProcessing = false;
    private ShaderMaterial _shaderMaterial; // Store the ShaderMaterial

    public override void _Ready()
        {
        if (_textureRect.Texture != null)
            {
            _originalTexture = _textureRect.Texture;
            _originalImage = _originalTexture.GetImage();
            }

        // --- IMPORTANT: Get a reference to the ShaderMaterial ---
        _shaderMaterial = (ShaderMaterial)_textureRect.Material;
        if (_shaderMaterial == null)
            {
            GD.PrintErr("TextureRect does not have a ShaderMaterial assigned in the editor!");
            // You could create a new ShaderMaterial here as a fallback, if desired:
            // _shaderMaterial = new ShaderMaterial();
            // _textureRect.Material = _shaderMaterial;
            return; // Exit if no ShaderMaterial is found to avoid errors
            }

        // Connect UI signals
        _saturationSlider.ValueChanged += (value) => QueueApplyEffects();
        _brightnessSlider.ValueChanged += (value) => QueueApplyEffects();
        _outlineCheckbox.Toggled += (pressed) => QueueApplyEffects();
        _outlineThicknessSlider.ValueChanged += (value) => QueueApplyEffects();
        _outlineColorPicker.ColorChanged += (color) => QueueApplyEffects();
        _ditheringCheckbox.Toggled += (pressed) => QueueApplyEffects();
        _ditheringSlider.ValueChanged += (value) => QueueApplyEffects();
        _colorReductionCheckbox.Toggled += (pressed) => QueueApplyEffects();
        _colorCountSpinBox.ValueChanged += (value) => QueueApplyEffects();
        _saveButton.Pressed += OnSaveButtonPressed;

        // Connect enable/disable toggles
        _enableSaturationCheckbox.Toggled += (pressed) => UpdateShaderParameters();
        _enableBrightnessCheckbox.Toggled += (pressed) => UpdateShaderParameters();

        _statusLabel.Text = "Ready";
        QueueApplyEffects();
        UpdateShaderParameters(); // Initial shader parameter update
        }

    private void QueueApplyEffects()
        {
            if (_isProcessing) return;
            _isProcessing = true;

            // Capture UI state for image processing effects (excluding shader controls)
            bool doColorReduction = _colorReductionCheckbox?.ButtonPressed ?? false;
            int colorCount = doColorReduction ? (int)(_colorCountSpinBox?.Value ?? 0) : 0;
            bool doOutline = _outlineCheckbox?.ButtonPressed ?? false;
            int outlineThickness = doOutline ? (int)(_outlineThicknessSlider?.Value ?? 0) : 0;
            Color outlineColor = doOutline ? (_outlineColorPicker?.Color ?? Colors.Black) : Colors.Black;
            bool doDithering = _ditheringCheckbox?.ButtonPressed ?? false;
            float ditheringStrength = doDithering ? (float)(_ditheringSlider?.Value ?? 0.0f) : 0.0f;

            Task.Run(() => ApplyEffectsAsync(doColorReduction, colorCount, doOutline, outlineThickness,
                outlineColor, doDithering, ditheringStrength));
        }

    private async Task ApplyEffectsAsync(bool doColorReduction, int colorCount, bool doOutline,
        int outlineThickness, Color outlineColor, bool doDithering, float ditheringStrength)
        {
        Image modifiedImage = (Image)_originalImage.Duplicate();

        if (doColorReduction)
            {
            CallDeferred(nameof(SetStatus), "Reducing colors...");
            modifiedImage = await Task.Run(() => ReduceColors(modifiedImage, colorCount));
            CallDeferred(nameof(SetStatus), "Color reduction complete.");
            }

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

    private void UpdateTexture(Image modifiedImage)
        {
        _textureRect.Texture = ImageTexture.CreateFromImage(modifiedImage);
        _isProcessing = false;
        SetStatus("Ready");
        UpdateShaderParameters(); // Update shader params after image update
        }

    private void UpdateShaderParameters()
        {
        if (_shaderMaterial == null) return; // Safety check

        // Set shader parameters based on UI controls
        _shaderMaterial.SetShaderParameter("enable_saturation", _enableSaturationCheckbox.ButtonPressed);
        _shaderMaterial.SetShaderParameter("saturation", _saturationSlider.Value);
        _shaderMaterial.SetShaderParameter("enable_brightness", _enableBrightnessCheckbox.ButtonPressed);
        _shaderMaterial.SetShaderParameter("brightness", _brightnessSlider.Value);
        }

    private void SetStatus(string text)
        {
        _statusLabel.Text = text;
        GD.Print(text);
        }

    private void OnSaveButtonPressed()
        {
        if (_textureRect.Texture == null) return;
        Image modifiedImage = ((ImageTexture)_textureRect.Texture).GetImage();
        FileDialog fileDialog = new FileDialog
            {
            FileMode = FileDialog.FileModeEnum.SaveFile,
            Filters = new string[] { "*.png ; PNG Images" },
            CurrentDir = "res://",
            CurrentFile = "modified_image.png"
            };
        fileDialog.FileSelected += (path) => SaveImageToFile(modifiedImage, path);
        AddChild(fileDialog);
        fileDialog.PopupCentered();
        }

    private void SaveImageToFile(Image image, string filePath)
        {
        Error err = image.SavePng(filePath);
        if (err != Error.Ok) GD.PrintErr("Error saving image: " + err);
        }

    // --- Image Processing Functions (no changes needed here) ---

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

    private Image ApplyDithering(Image image, float strength)
        {
        int width = image.GetWidth();
        int height = image.GetHeight();
        byte[] originalData = image.GetData();
        byte[] ditheredData = new byte[originalData.Length];
        Array.Copy(originalData, ditheredData, originalData.Length);

        // Floyd-Steinberg dithering
        for (int y = 0; y < height; y++)
        {
            // Create a local error buffer for this row. CRITICAL for thread safety.
            Color[] errorBuffer = new Color[width];

            for (int x = 0; x < width; x++)
                {
                int index = (y * width + x) * 4;

                // Apply any accumulated error from the errorBuffer.
                Color oldPixel = GetColorFromBytes(ditheredData, index) + errorBuffer[x];
                oldPixel = ClampColor(oldPixel);

                Color newPixel = FindClosestPaletteColor(oldPixel);
                SetColorToBytes(ditheredData, index, newPixel);

                Color error = oldPixel - newPixel;

                // Distribute the error to neighboring pixels, CORRECTLY handling boundaries.

                // Right neighbor (x + 1):  Add to the errorBuffer for the *current* row.
                if (x + 1 < width)
                    {
                    errorBuffer[x + 1] += error * (7.0f / 16.0f) * strength;
                    }

                // The next row (y + 1):  Directly modify ditheredData, NOT errorBuffer.
                if (y + 1 < height)
                    {
                    if (x - 1 >= 0)
                        {
                        int nextRowIndex = ((y + 1) * width + (x - 1)) * 4;
                        Color currentColor = GetColorFromBytes(ditheredData, nextRowIndex);
                        SetColorToBytes(ditheredData, nextRowIndex, ClampColor(currentColor + error * (3.0f / 16.0f) * strength));
                        }
                    if (x + 1 < width) // Corrected: x + 1, not x
                        {
                        int nextRowIndex = ((y + 1) * width + (x + 1)) * 4;
                        Color currentColor = GetColorFromBytes(ditheredData, nextRowIndex);
                        SetColorToBytes(ditheredData, nextRowIndex, ClampColor(currentColor + error * (1.0f / 16.0f) * strength));
                        }
                    // Center pixel on next row
                    int nextRowIndexCenter = ((y + 1) * width + x) * 4;
                    Color currentColorCenter = GetColorFromBytes(ditheredData, nextRowIndexCenter);
                    SetColorToBytes(ditheredData, nextRowIndexCenter, ClampColor(currentColorCenter + error * (5.0f / 16.0f) * strength));
                    }
                }
        };

        return Image.CreateFromData(width, height, false, Image.Format.Rgba8, ditheredData);
        }

    private Image ReduceColors(Image image, int numColors)
        {
        List<Color> palette = KMeansClustering(image, numColors);
        int width = image.GetWidth();
        int height = image.GetHeight();
        byte[] originalData = image.GetData();
        byte[] reducedData = new byte[originalData.Length];

        Parallel.For(0, height, y =>
        {
            for (int x = 0; x < width; x++)
                {
                int index = (y * width + x) * 4;
                Color originalColor = GetColorFromBytes(originalData, index);

                if (originalColor.A == 0)
                    {
                    Array.Copy(originalData, index, reducedData, index, 4);
                    }
                else
                    {
                    Color closestColor = FindClosestColor(originalColor, palette);
                    closestColor.A = originalColor.A;
                    SetColorToBytes(reducedData, index, closestColor);
                    }
                }
        });

        return Image.CreateFromData(width, height, false, Image.Format.Rgba8, reducedData);
        }

    private List<Color> KMeansClustering(Image image, int k)
     {
        if(_colorCountSpinBox.Value == 0) return new List<Color>() { new Color(0, 0, 0, 0) };

        List<Color> colors = new List<Color>();
        int width = image.GetWidth();
        int height = image.GetHeight();
        byte[] data = image.GetData();

        for (int y = 0; y < height; y++)
            {
            for (int x = 0; x < width; x++)
                {
                int index = (y * width + x) * 4;
                if (data[index + 3] > 0)
                    {
                    colors.Add(GetColorFromBytes(data, index));
                    }
                }
            }

        if (colors.Count == 0) return new List<Color>() { new Color(0, 0, 0, 0) };

        List<Color> centroids = new List<Color>();
        Random random = new Random();
        for (int i = 0; i < k; i++) centroids.Add(colors[random.Next(colors.Count)]);

        int maxIterations = 20;
        for (int iter = 0; iter < maxIterations; iter++)
            {
            List<Color>[] clusters = new List<Color>[k];
            for (int i = 0; i < k; i++) clusters[i] = new List<Color>();
            foreach (Color color in colors) clusters[FindNearestCentroidIndex(color, centroids)].Add(color);
            List<Color> newCentroids = new List<Color>();
            for (int i = 0; i < k; i++)
                {
                newCentroids.Add(clusters[i].Count > 0 ? CalculateMeanColor(clusters[i]) : colors[random.Next(colors.Count)]);
                }
            centroids = newCentroids;
            }
        return centroids;
        }

    // --- Helper Functions ---
    private int FindNearestCentroidIndex(Color color, List<Color> centroids)
        {
        int nearestIndex = 0;
        float minDistance = float.MaxValue;
        for (int i = 0; i < centroids.Count; i++)
            {
            float distance = ColorDistance(color, centroids[i]);
            if (distance < minDistance)
                {
                minDistance = distance;
                nearestIndex = i;
                }
            }
        return nearestIndex;
        }

    private Color CalculateMeanColor(List<Color> colors)
        {
        float sumR = 0, sumG = 0, sumB = 0, sumA = 0;
        foreach (Color color in colors)
            {
            sumR += color.R; sumG += color.G; sumB += color.B; sumA += color.A;
            }
        int count = colors.Count;
        return new Color(sumR / count, sumG / count, sumB / count, sumA / count);
        }

    private float ColorDistance(Color c1, Color c2) => Mathf.Sqrt(Mathf.Pow(c1.R - c2.R, 2) + Mathf.Pow(c1.G - c2.G, 2) + Mathf.Pow(c1.B - c2.B, 2) + Mathf.Pow(c1.A - c2.A, 2));
    private Color FindClosestColor(Color targetColor, List<Color> palette)
        {
        Color closestColor = palette[0];
        float minDistance = ColorDistance(targetColor, closestColor);
        foreach (Color paletteColor in palette)
            {
            float distance = ColorDistance(targetColor, paletteColor);
            if (distance < minDistance)
                {
                minDistance = distance;
                closestColor = paletteColor;
                }
            }
        return closestColor;
        }
    private Color GetColorFromBytes(byte[] data, int index) => new Color(data[index] / 255.0f, data[index + 1] / 255.0f, data[index + 2] / 255.0f, data[index + 3] / 255.0f);
    private void SetColorToBytes(byte[] data, int index, Color color)
        {
        data[index] = (byte)(color.R * 255);
        data[index + 1] = (byte)(color.G * 255);
        data[index + 2] = (byte)(color.B * 255);
        data[index + 3] = (byte)(color.A * 255);
        }
    private Color FindClosestPaletteColor(Color color) => new Color(Mathf.Round(color.R * 255) / 255, Mathf.Round(color.G * 255) / 255, Mathf.Round(color.B * 255) / 255, color.A);
    private Color ClampColor(Color color) => new Color(Mathf.Clamp(color.R, 0, 1), Mathf.Clamp(color.G, 0, 1), Mathf.Clamp(color.B, 0, 1), color.A);
    }