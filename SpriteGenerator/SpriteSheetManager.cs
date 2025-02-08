// MainScene.cs (Attach this to the root node of your scene)

using Godot;
using System;
using System.Collections.Generic;
using Image = Godot.Image; // Avoid naming conflicts

public partial class SpriteSheetManager : PanelContainer
{
    [Export]
    private TextureRect _textureRect; // Drag your TextureRect node here in the editor

    [Export]
    private HSlider _saturationSlider; // Sliders for your UI controls

    [Export]
    private HSlider _brightnessSlider;

    [Export]
    private CheckBox _outlineCheckbox;

    [Export]
    private HSlider _outlineThicknessSlider;

    [Export]
    private ColorPickerButton _outlineColorPicker;

    [Export]
    private CheckBox _ditheringCheckbox;

    [Export]
    private HSlider _ditheringSlider; // Control the strength

    [Export]
    private CheckBox _colorReductionCheckbox;

    [Export]
    private SpinBox _colorCountSpinBox;  // Use a SpinBox for integer input

    [Export]
    private Button _saveButton;


    private Texture2D _originalTexture;  // Store the original, unmodified texture
    private Image _originalImage; //Store the original image.


    public override void _Ready()
    {
        // Load the initial texture
        if (_textureRect.Texture != null)
        {
            _originalTexture = _textureRect.Texture;
            _originalImage = _originalTexture.GetImage();
        }

        // Connect UI signals to handler functions.
        // Handle null checks gracefully.
        if (_saturationSlider != null) _saturationSlider.ValueChanged += OnSaturationChanged;
        if (_brightnessSlider != null) _brightnessSlider.ValueChanged += OnBrightnessChanged;
        if (_outlineCheckbox != null) _outlineCheckbox.Toggled += OnOutlineToggled;
        if (_outlineThicknessSlider != null) _outlineThicknessSlider.ValueChanged += OnOutlineThicknessChanged;
        if (_outlineColorPicker != null) _outlineColorPicker.ColorChanged += OnOutlineColorChanged;
        if (_ditheringCheckbox != null) _ditheringCheckbox.Toggled += OnDitheringToggled;
        if (_ditheringSlider != null) _ditheringSlider.ValueChanged += OnDitheringChanged;
        if (_colorReductionCheckbox != null) _colorReductionCheckbox.Toggled += OnColorReductionToggled;
        if (_colorCountSpinBox != null) _colorCountSpinBox.ValueChanged += OnColorCountChanged;
        if (_saveButton != null) _saveButton.Pressed += OnSaveButtonPressed;

        // Call apply effects initially in case of default values.
        ApplyEffects();
    }

    private void ApplyEffects()
    {
        if (_originalTexture == null) return;

        // Create a copy of the original image to work with.  CRITICAL: This prevents modifying the original resource.
        Image modifiedImage = (Image)_originalImage.Duplicate();


        // 1. Color Reduction (Process BEFORE shader effects)
        if (_colorReductionCheckbox != null && _colorReductionCheckbox.ButtonPressed)
        {
            if (_colorCountSpinBox != null)
            {
                modifiedImage = ReduceColors(modifiedImage, (int)_colorCountSpinBox.Value);
            }
        }

        // 2. Apply Shader Effects (Saturation, Brightness)
        // We'll use a ShaderMaterial with a shader.

        if (!(_textureRect.Material is ShaderMaterial))
        {
            _textureRect.Material = new ShaderMaterial();
        }
        ShaderMaterial mat = (ShaderMaterial)_textureRect.Material;

        if (mat.Shader == null)
        {
            mat.Shader = new Shader();
            mat.Shader.Code = GetShaderCode();
        }

        // Set shader parameters.  Handle null checks for safety.
        mat.SetShaderParameter("saturation", _saturationSlider != null ? (float)_saturationSlider.Value : 1.0f);
        mat.SetShaderParameter("brightness", _brightnessSlider != null ? (float)_brightnessSlider.Value : 1.0f);


        // 3. Outline (Image-based, after color reduction and dithering)
        if (_outlineCheckbox != null && _outlineCheckbox.ButtonPressed)
            {
            if (_outlineThicknessSlider != null && _outlineColorPicker != null)
                {   //No Duplicate needed any more.
                modifiedImage = AddOutline(modifiedImage, (int)_outlineThicknessSlider.Value, _outlineColorPicker.Color);
                }
            }

        // 4. Dithering
        if (_ditheringCheckbox != null && _ditheringCheckbox.ButtonPressed) { /* ... */ }

        // 5. Update the TextureRect with the modified image.
        ImageTexture newTexture = ImageTexture.CreateFromImage(modifiedImage);
        _textureRect.Texture = newTexture;
     }



    private void OnSaturationChanged(double value) => ApplyEffects();
    private void OnBrightnessChanged(double value) => ApplyEffects();
    private void OnOutlineToggled(bool pressed) => ApplyEffects();
    private void OnOutlineThicknessChanged(double value) => ApplyEffects();
    private void OnOutlineColorChanged(Color color) => ApplyEffects();
    private void OnDitheringToggled(bool pressed) => ApplyEffects();
    private void OnDitheringChanged(double value) => ApplyEffects();
    private void OnColorReductionToggled(bool pressed) => ApplyEffects();
    private void OnColorCountChanged(double value) => ApplyEffects();


    private void OnSaveButtonPressed()
    {
        if (_textureRect.Texture == null) return;

        // Get the modified image from the current texture
        Image modifiedImage = ((ImageTexture)_textureRect.Texture).GetImage();

        // Use a FileDialog for a better user experience (Optional, but recommended)
        FileDialog fileDialog = new FileDialog
        {
            FileMode = FileDialog.FileModeEnum.SaveFile,
            Filters = new string[] { "*.png ; PNG Images" },
            CurrentDir = "res://", // Or any default directory
            CurrentFile = "modified_image.png"
        };
        fileDialog.FileSelected += (path) => SaveImageToFile(modifiedImage, path);
        AddChild(fileDialog);
        fileDialog.PopupCentered();
    }

    private void SaveImageToFile(Image image, string filePath)
    {
        Error err = image.SavePng(filePath);
        if (err != Error.Ok)
        {
            GD.PrintErr("Error saving image: " + err);
            // Optionally show an error message to the user.
        }
    }

    private string GetShaderCode()
    {
        return @"
			shader_type canvas_item;

			uniform float saturation : hint_range(0.0, 2.0) = 1.0;
			uniform float brightness : hint_range(0.0, 2.0) = 1.0;

			void fragment() {
				vec4 color = texture(TEXTURE, UV);
				// Convert to HSL
				vec3 hsl = rgb_to_hsl(color.rgb);
				// Modify saturation and lightness
				hsl.y *= saturation;
				hsl.z *= brightness;
				// Convert back to RGB
				color.rgb = hsl_to_rgb(hsl);
				COLOR = color;
			}

			// Helper functions (from Godot's shader documentation)
			vec3 rgb_to_hsl(vec3 c) {
				float h = 0.0;
				float s = 0.0;
				float l = 0.0;
				float r = c.r;
				float g = c.g;
				float b = c.b;
				float cMin = min(r, min(g, b));
				float cMax = max(r, max(g, b));
				l = (cMax + cMin) / 2.0;
				if (cMax > cMin) {
					float cDelta = cMax - cMin;
					s = l < 0.5 ? cDelta / (cMax + cMin) : cDelta / (2.0 - (cMax + cMin));
					if (r == cMax) {
						h = (g - b) / cDelta;
					} else if (g == cMax) {
						h = 2.0 + (b - r) / cDelta;
					} else if (b == cMax) {
						h = 4.0 + (r - g) / cDelta;
					}
					h = mod(h / 6.0, 1.0);
				}
				return vec3(h, s, l);
			}

			vec3 hsl_to_rgb(vec3 c) {
				float h = c.x;
				float s = c.y;
				float l = c.z;
				float ret_r = l;
				float ret_g = l;
				float ret_b = l;
				if (s != 0.0) {
					float q = l < 0.5 ? l * (1.0 + s) : l + s - (l * s);
					float p = 2.0 * l - q;
					float h_k = h;
					float t_r = h_k + (1.0 / 3.0);
					float t_g = h_k;
					float t_b = h_k - (1.0 / 3.0);
					t_r = mod(t_r, 1.0);
					t_g = mod(t_g, 1.0);
					t_b = mod(t_b, 1.0);
					ret_r = t_r < (1.0 / 6.0) ? p + ((q - p) * 6.0 * t_r) : (t_r < 0.5 ? q : (t_r < (2.0 / 3.0) ? p + ((q - p) * 6.0 * ((2.0 / 3.0) - t_r)) : p));
					ret_g = t_g < (1.0 / 6.0) ? p + ((q - p) * 6.0 * t_g) : (t_g < 0.5 ? q : (t_g < (2.0 / 3.0) ? p + ((q - p) * 6.0 * ((2.0 / 3.0) - t_g)) : p));
					ret_b = t_b < (1.0 / 6.0) ? p + ((q - p) * 6.0 * t_b) : (t_b < 0.5 ? q : (t_b < (2.0 / 3.0) ? p + ((q - p) * 6.0 * ((2.0 / 3.0) - t_b)) : p));
				}
				return vec3(ret_r, ret_g, ret_b);
			}
		";
    }

    //Image Based Effects:
    private Image AddOutline(Image image, int thickness, Color color)
        {
        int width = image.GetWidth();
        int height = image.GetHeight();

        byte[] originalData = image.GetData();  // Get raw byte data (RGBA format)
        byte[] outlinedData = new byte[originalData.Length]; // Create byte array for output
        Array.Copy(originalData, outlinedData, originalData.Length); //Copy data from original


        // Pre-calculate color components
        byte outlineR = (byte)(color.R * 255);
        byte outlineG = (byte)(color.G * 255);
        byte outlineB = (byte)(color.B * 255);
        byte outlineA = (byte)(color.A * 255);

        for (int y = 0; y < height; y++)
            {
            for (int x = 0; x < width; x++)
                {
                int originalIndex = (y * width + x) * 4; // Index into the byte array (RGBA)
                                                         //Fast early exit
                if (originalData[originalIndex + 3] == 0)
                    {
                    continue;
                    }

                // Iterate only within the outline thickness, using Max/Min for bounds checking
                for (int oy = Math.Max(0, y - thickness); oy <= Math.Min(height - 1, y + thickness); oy++)
                    {
                    for (int ox = Math.Max(0, x - thickness); ox <= Math.Min(width - 1, x + thickness); ox++)
                        {
                        //Skip current pixel.
                        if (ox == x && oy == y) continue;

                        int outlinedIndex = (oy * width + ox) * 4;
                        // Only set the outline if the target pixel is currently transparent
                        if (outlinedData[outlinedIndex + 3] == 0)
                            {
                            outlinedData[outlinedIndex + 0] = outlineR; // Red
                            outlinedData[outlinedIndex + 1] = outlineG; // Green
                            outlinedData[outlinedIndex + 2] = outlineB; // Blue
                            outlinedData[outlinedIndex + 3] = outlineA; // Alpha
                            }
                        }
                    }
                }
            }
        Image outlinedImage = Image.CreateFromData(width, height, false, Image.Format.Rgba8, outlinedData);
        return outlinedImage;
        }

    private Image ApplyDithering(Image image, float strength)
    {
        //image.Lock();
        int width = image.GetWidth();
        int height = image.GetHeight();
        Image ditheredImage = (Image)image.Duplicate(); // Work on a copy
        //ditheredImage.Lock();

        // Basic Floyd-Steinberg dithering
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color oldPixel = ditheredImage.GetPixel(x, y);
                Color newPixel = FindClosestPaletteColor(oldPixel); // Simple quantization
                ditheredImage.SetPixel(x, y, newPixel);

                Color error = oldPixel - newPixel;

                // Distribute the error
                if (x + 1 < width)
                    ditheredImage.SetPixel(x + 1, y, ClampColor(ditheredImage.GetPixel(x + 1, y) + error * (7.0f / 16.0f) * strength));
                if (x - 1 >= 0 && y + 1 < height)
                    ditheredImage.SetPixel(x - 1, y + 1, ClampColor(ditheredImage.GetPixel(x - 1, y + 1) + error * (3.0f / 16.0f) * strength));
                if (y + 1 < height)
                    ditheredImage.SetPixel(x, y + 1, ClampColor(ditheredImage.GetPixel(x, y + 1) + error * (5.0f / 16.0f) * strength));
                if (x + 1 < width && y + 1 < height)
                    ditheredImage.SetPixel(x + 1, y + 1, ClampColor(ditheredImage.GetPixel(x + 1, y + 1) + error * (1.0f / 16.0f) * strength));
            }
        }
        //ditheredImage.Unlock();
        //image.Unlock();
        return ditheredImage;
    }

    // Helper function for dithering
    private Color FindClosestPaletteColor(Color color)
    {
        // This is a VERY simplified quantization.  For better dithering,
        // you'd use a pre-defined palette (e.g., a 256-color palette).
        return new Color(
            Mathf.Round(color.R * 255) / 255,
            Mathf.Round(color.G * 255) / 255,
            Mathf.Round(color.B * 255) / 255,
            color.A
        );
    }

    // Helper function to keep color values within 0-1 range
    private Color ClampColor(Color color)
    {
        return new Color(
            Mathf.Clamp(color.R, 0, 1),
            Mathf.Clamp(color.G, 0, 1),
            Mathf.Clamp(color.B, 0, 1),
            color.A // Preserve alpha
        );
    }

    private Image ReduceColors(Image image, int numColors)
        {
        // 1. Build a color palette using K-means clustering (ignoring transparent pixels).
        List<Color> palette = KMeansClustering(image, numColors);

        // 2. Map each pixel in the image to the nearest color in the palette (preserving transparency).
        Image reducedImage = (Image)image.Duplicate(); // Work on a copy, as always
        //reducedImage.Lock();
        //image.Lock();

        for (int x = 0; x < reducedImage.GetWidth(); x++)
            {
            for (int y = 0; y < reducedImage.GetHeight(); y++)
                {
                Color originalColor = image.GetPixel(x, y);

                // *** Crucial change: Preserve transparency ***
                if (originalColor.A == 0)
                    {
                    reducedImage.SetPixel(x, y, originalColor); // Keep transparent pixels as-is
                    }
                else
                    {
                    Color closestColor = FindClosestColor(originalColor, palette);
                    // Ensure we don't modify the alpha channel of the closest color
                    closestColor.A = originalColor.A; // Preserve the original alpha
                    reducedImage.SetPixel(x, y, closestColor);
                    }
                }
            }

        //reducedImage.Unlock();
        //image.Unlock();
        return reducedImage;
    }

    private List<Color> KMeansClustering(Image image, int k)
        {
        // Convert image data to a list of colors (EXCLUDING fully transparent pixels).
        List<Color> colors = new List<Color>();
        //image.Lock();
        for (int x = 0; x < image.GetWidth(); x++)
            {
            for (int y = 0; y < image.GetHeight(); y++)
                {
                Color pixelColor = image.GetPixel(x, y);
                if (pixelColor.A > 0) // *** KEY CHANGE: Consider only non-transparent pixels ***
                    {
                    colors.Add(pixelColor);
                    }
                }
            }
        //image.Unlock();

        // Handle the edge case where the image is entirely transparent.
        if (colors.Count == 0)
            {
            // Return a palette with a single transparent color.  This prevents errors.
            return new List<Color>() { new Color(0, 0, 0, 0) };
            }

        // 1. Initialize centroids randomly.
        List<Color> centroids = new List<Color>();
        Random random = new Random();
        for (int i = 0; i < k; i++)
            {
            centroids.Add(colors[random.Next(colors.Count)]);
            }

        // 2. Iterate until convergence (or a maximum number of iterations).
        int maxIterations = 100; // Prevent infinite loops
        for (int iter = 0; iter < maxIterations; iter++)
            {
            // 2a. Assign each color to the nearest centroid.
            List<Color>[] clusters = new List<Color>[k];
            for (int i = 0; i < k; i++)
                {
                clusters[i] = new List<Color>();
                }

            foreach (Color color in colors)
                {
                int nearestCentroidIndex = FindNearestCentroidIndex(color, centroids);
                clusters[nearestCentroidIndex].Add(color);
                }

            // 2b. Update centroids to be the mean of their clusters.
            List<Color> newCentroids = new List<Color>();
            for (int i = 0; i < k; i++)
                {
                if (clusters[i].Count > 0)
                    {
                    newCentroids.Add(CalculateMeanColor(clusters[i]));
                    }
                else
                    {
                    // If a cluster is empty, re-initialize the centroid randomly.
                    newCentroids.Add(colors[random.Next(colors.Count)]);
                    }
                }

            // 2c. Check for convergence (if centroids haven't changed much).
            bool converged = true;
            for (int i = 0; i < k; i++)
                {
                if (ColorDistance(centroids[i], newCentroids[i]) > 0.001f) // Small threshold
                    {
                    converged = false;
                    break;
                    }
                }

            centroids = newCentroids; // Update centroids

            if (converged)
                {
                break;
                }
            }

        return centroids;
        }

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
        float sumR = 0;
        float sumG = 0;
        float sumB = 0;
        float sumA = 0; // Include alpha in the average

        foreach (Color color in colors)
        {
            sumR += color.R;
            sumG += color.G;
            sumB += color.B;
            sumA += color.A;
        }

        int count = colors.Count;
        return new Color(sumR / count, sumG / count, sumB / count, sumA / count);
    }

    private float ColorDistance(Color c1, Color c2)
    {
        // Simple Euclidean distance in RGBA space.
        float dr = c1.R - c2.R;
        float dg = c1.G - c2.G;
        float db = c1.B - c2.B;
        float da = c1.A - c2.A;
        return Mathf.Sqrt(dr * dr + dg * dg + db * db + da * da);
    }

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
}