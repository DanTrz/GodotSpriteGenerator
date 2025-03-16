using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Godot;
using Image = Godot.Image;

public partial class ImageEditorMainPanel : PanelContainer
{
    [Export] private ImageEditor _ImgEditor;
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
    [Export] private Label _effectStatusLabel;
    [Export] public MarginContainer EffectStatusMainPanel;

    [Export] public CheckButton UseExternalPaletteChkBtn;
    [Export] public PaletteLoader PaletteLoaderPanel;

    // Add toggles for enabling/disabling shader effects
    [Export] private CheckBox _enableSaturationCheckbox;
    [Export] private CheckBox _enableBrightnessCheckbox;

    [Export] public Button LoadExternalImg;


    private Godot.Collections.Array<Color> _currentPaletteColors = new();
    private Texture2D _originalTexture;
    private Image _originalImage;

    private bool _isFirstRun = true;


    private bool _useExternalPalette = false;
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
        if (_ImgEditor.ImgTextRect.Texture != null)
        {
            _originalTexture = _ImgEditor.ImgTextRect.Texture;
            _originalImage = _originalTexture.GetImage();
        }

        //Connect UI Sigansl for most of the effects
        _saturationSlider.ValueChanged += (value) => SetImgProcessorShaderParams();
        _brightnessSlider.ValueChanged += (value) => SetImgProcessorShaderParams();
        _outlineCheckbox.Toggled += (pressed) => SetImgProcessorShaderParams();
        _outlineThicknessSlider.ValueChanged += (value) => SetImgProcessorShaderParams();
        _outlineColorPicker.ColorChanged += (color) => SetImgProcessorShaderParams();
        _enableSaturationCheckbox.Toggled += (pressed) => SetImgProcessorShaderParams();
        _enableBrightnessCheckbox.Toggled += (pressed) => SetImgProcessorShaderParams();
        _colorCountSpinBox.ValueChanged += (value) => SetImgProcessorShaderParams();
        _colorReductionCheckbox.Toggled += (pressed) => SetImgProcessorShaderParams();

        _saveButton.Pressed += OnSaveButtonPressed;
        LoadExternalImg.Pressed += async () => await OnLoadExternalImgBtnPressed();

        GlobalEvents.Instance.OnPaletteChanged += OnPaletteChanged;
        GlobalEvents.Instance.OnEffectsChangesStarted += OnEffectsChangesStarted;
        GlobalEvents.Instance.OnEffectsChangesEnded += OnEffectsChangesEnded;


        UseExternalPaletteChkBtn.Toggled += OnUseExternalPaletteBtnToggled;
        PaletteLoaderPanel.Visible = false;


        _effectStatusLabel.Text = "Ready";
        EffectStatusMainPanel.Visible = false;

        UpdateUIElementsOnLoad();

        _isFirstRun = false;

        SetImgProcessorShaderParams();
        // Initial shader parameter update

    }

    private void OnEffectsChangesStarted(string fromNode)
    {
        GD.PrintT("Run: OnEffectsChangesStarted");
        _effectStatusLabel.Text = "Processing Image updates....";
        EffectStatusMainPanel.Visible = true;
    }


    private void OnEffectsChangesEnded(string fromNode, Godot.Collections.Array<Color> list)
    {
        GD.PrintT("Run: OnEffectsChangesEnded");
        _effectStatusLabel.Text = "Effects Applied !!! ";
        EffectStatusMainPanel.Visible = false;
        PaletteLoaderPanel.UpdatePaletteListGrid(list);
    }


    private void OnPaletteChanged(Godot.Collections.Array<Color> list)
    {
        GD.Print("Palette changed");

        int colorCount = list.Count;
        _colorCountSpinBox.Value = list.Count;

        _currentPaletteColors = list;
        //4. We Update the Shader Parameters in the ImageEditorMainPanel

        SetImgProcessorShaderParams();
    }


    private async Task OnLoadExternalImgBtnPressed()
    {
        using Godot.FileDialog fileDialog = new Godot.FileDialog
        {
            FileMode = FileDialog.FileModeEnum.OpenFile,
            Filters = new string[] { "*.png, *.jpg, *.jpeg ; Images" },
            Access = FileDialog.AccessEnum.Filesystem

        };

        AddChild(fileDialog);

        fileDialog.CurrentDir = GlobalUtil.SaveFolderPath; //Set this after adding Child to Scene

        fileDialog.PopupCentered();

        await ToSignal(fileDialog, FileDialog.SignalName.FileSelected);


        string selectedFiled = fileDialog.CurrentDir + "/" + fileDialog.CurrentFile;

        await LoadTextureFromImgFile(selectedFiled);

        UpdateUIElementsOnLoad();

    }

    private async Task LoadTextureFromImgFile(string path)
    {
        //PREVIOUS CODE START ----------
        // Image imgToLoad = Image.LoadFromFile(path);

        // await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        // _ImgEditor.ImgTextRect.Texture = ImageTexture.CreateFromImage(imgToLoad);
        // _ImgEditor.NumColors = _ImgEditor.GetUniqueColorsCount(imgToLoad).Result;

        //PREVIOUS CODE END ----------


        //NEW CODE - TRYING TO SET HIGH QUALITY IMPORT TEXTURE AND IMG
        Image imgToLoad = Image.LoadFromFile(path);

        if (imgToLoad.IsCompressed())
        {
            // Decompress if necessary (it shouldn't be compressed if it's a PNG/JPG/etc.,
            imgToLoad.Decompress();
        }

        if (imgToLoad.GetFormat() != Image.Format.Rgba8)
        {
            // Convert to a high-quality format. 
            imgToLoad.Convert(Image.Format.Rgba8);
        }

        // Generate mipmaps.  (Do this *before* creating the ImageTexture.)
        imgToLoad.GenerateMipmaps();

        // Wait for the next process frame to ensure the image is fully processed.
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        // Create the ImageTexture.  Specify that mipmaps are enabled.
        ImageTexture texture = ImageTexture.CreateFromImage(imgToLoad);

        if (_ImgEditor != null && _ImgEditor.ImgTextRect != null)
        {
            _ImgEditor.ImgTextRect.Texture = texture;
            // Ensure GetUniqueColorsCount is not blocking the main thread.
            _ImgEditor.NumColors = await Task.Run(() => _ImgEditor.GetUniqueColorsCount(imgToLoad));
        }
        else
        {
            GD.PrintErr("_ImgEditor or _ImgEditor.ImgTextRect is null!");  // Debugging: Check initialization
        }

    }

    public void UpdateUIElementsOnLoad()
    {
        if (_ImgEditor.ImgTextRect.Texture != null)
        {
            _originalTexture = _ImgEditor.ImgTextRect.Texture;
            _originalImage = _originalTexture.GetImage();
        }
        //_colorCountSpinBox.MaxValue = _ImgEditor.NumColors; //Mathf.Clamp(value, min, max);
        _colorCountSpinBox.MaxValue = Mathf.Clamp(_ImgEditor.NumColors, 1, _ImgEditor.MAX_PALETTE_SIZE);

        _colorCountSpinBox.Value = _ImgEditor.NumColors;
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

    }

    private void SetImgProcessorShaderParams()
    {
        if (_isFirstRun) return;
        _ImgEditor.EnableColorReduction = _colorReductionCheckbox?.ButtonPressed ?? false;
        _ImgEditor.NumColors = (int)(_colorCountSpinBox?.Value ?? 0);
        //int colorCount = (int)(_colorCountSpinBox?.Value ?? 0);

        _useExternalPalette = UseExternalPaletteChkBtn?.ButtonPressed ?? false;
        _ImgEditor._useExternalPalette = _useExternalPalette;
        _ImgEditor.ShaderPalette = _currentPaletteColors;
        _ImgEditor.EnableSaturation = _enableSaturationCheckbox?.ButtonPressed ?? false;
        _ImgEditor.Saturation = (float)(_saturationSlider?.Value ?? 0.0f);

        _ImgEditor.EnableBrightness = _enableBrightnessCheckbox?.ButtonPressed ?? false;
        _ImgEditor.Brightness = (float)(_brightnessSlider?.Value ?? 0.0f);

        if (!_useExternalPalette)
        {
            //List<Color> shaderPalette = _ImgEditor.GetShaderPalette(colorCount);
            //TODO: Determine if this is needed #BUG
            _currentPaletteColors = _ImgEditor.GetOriginalTexturePalette();
            _ImgEditor.ShaderPalette = _currentPaletteColors;

            PaletteLoaderPanel.Visible = true;
        }

        _ImgEditor.UpdateShaderParameters();
        PaletteLoaderPanel.UpdatePaletteListGrid(_currentPaletteColors);
    }

    private void OnUseExternalPaletteBtnToggled(bool toggledOn)
    {
        //PaletteLoaderPanel.Visible = UseExternalPaletteChkBtn.ButtonPressed;
        UseExternalPaletteChkBtn.Text = UseExternalPaletteChkBtn.ButtonPressed.ToString();
        SetImgProcessorShaderParams();
    }


    private async Task ApplyEffectsAsync(bool doColorReduction, int colorCount, bool doOutline,
        int outlineThickness, Color outlineColor, bool doDithering, float ditheringStrength)
    {

        //I always start with a copy of the original image, preserving the original for "undo" (to be implemented)
        Image modifiedImage = (Image)_originalImage.Duplicate(); //First code version (working)

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

    //Update UI Label with current status of effects processing
    private void SetStatus(string text)
    {
        _effectStatusLabel.Text = text;
        GD.Print(text);
    }

    //Save new Image to folder
    private void OnSaveButtonPressed()
    {
        if (_ImgEditor.ImgTextRect.Texture == null) return;
        Image modifiedImage = (Image)_ImgEditor.ImgTextRect.Texture.GetImage();

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
        _ImgEditor.ImgTextRect.Texture = ImageTexture.CreateFromImage(modifiedImage);

        //-> Update shader params after image update with other C# code effects.
        _ImgEditor.UpdateShaderParameters(); // Shader changes always last to ensure we will not get mixed up with other effects

        IsEffectProcessing = false;
        SetStatus("Ready");

    }

    public void OnSaveData(SaveGameData newSaveGameData)
    {
        GD.PrintT("Started OnSaveData from:", this.Name);
        newSaveGameData.SaturationIsOn = _enableSaturationCheckbox.ButtonPressed;
        newSaveGameData.SaturationSliderValue = (float)_saturationSlider.Value;
        newSaveGameData.BrightnessIsOn = _enableBrightnessCheckbox.ButtonPressed;
        newSaveGameData.BrightnessSliderValue = (float)_brightnessSlider.Value;
        newSaveGameData.OutlineIsOn = _outlineCheckbox.ButtonPressed;
        newSaveGameData.OutlineThicknessSliderValue = (float)_outlineThicknessSlider.Value;
        newSaveGameData.OutlineColor = _outlineColorPicker.Color;
        newSaveGameData.ColorReductionIsOn = _colorReductionCheckbox.ButtonPressed;
        newSaveGameData.ColorReductionValue = (float)_colorCountSpinBox.Value;

    }

    public async void OnLoadData(SaveGameData newLoadData)
    {

        GD.PrintT("Started OnLoadData from:", this.Name);
        UpdateUIElementsOnLoad();
        UpdateTexture(_originalImage);

        _enableSaturationCheckbox.ButtonPressed = newLoadData.SaturationIsOn;
        _saturationSlider.Value = newLoadData.SaturationSliderValue;
        _enableBrightnessCheckbox.ButtonPressed = newLoadData.BrightnessIsOn;
        _brightnessSlider.Value = newLoadData.BrightnessSliderValue;
        _outlineCheckbox.ButtonPressed = newLoadData.OutlineIsOn;
        _outlineThicknessSlider.Value = newLoadData.OutlineThicknessSliderValue;
        _outlineColorPicker.Color = newLoadData.OutlineColor;
        _colorReductionCheckbox.ButtonPressed = newLoadData.ColorReductionIsOn;
        _colorCountSpinBox.Value = newLoadData.ColorReductionValue;

        SetImgProcessorShaderParams();

        if (newLoadData.OutlineIsOn)
        {
            await ApplyEffectsAsync(
                newLoadData.ColorReductionIsOn,
                (int)newLoadData.ColorReductionValue,
                newLoadData.OutlineIsOn,
                (int)newLoadData.OutlineThicknessSliderValue,
                newLoadData.OutlineColor,
                false,
                0);
        }

    }

    public override void _ExitTree()
    {
        GlobalEvents.Instance.OnPaletteChanged -= OnPaletteChanged;
    }


}
