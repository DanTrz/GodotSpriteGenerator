using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Image = Godot.Image;

public partial class ImageEditorMainPanel : PanelContainer
{
    [Export] private ImageEditor _ImgEditor;
    [Export] private HSlider _saturationSlider;
    [Export] private Label _saturationValueLabel;
    [Export] private Label _brightnessValueLabel;
    [Export] private Label _contrastValueLabel;
    [Export] private Label _outlineLabel;
    [Export] private HSlider _brightnessSlider;
    [Export] private HSlider _contrastSlider;
    [Export] private CheckBox _outlineCheckbox;
    [Export] private HSlider _outlineThicknessSlider;

    [Export] private Button _resetImgCorrectionBtn;
    [Export] private ColorPickerButton _outlineColorPicker;
    [Export] private CheckBox _colorReductionCheckbox;
    [Export] private SpinBox _colorCountSpinBox;
    [Export] private Button _saveButton;
    [Export] private Label _effectStatusLabel;
    [Export] public MarginContainer EffectStatusMainPanel;

    [Export] public CheckButton UseExternalPaletteChkBtn;
    [Export] public PaletteLoader PaletteLoaderPanel;

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
        _saturationSlider.ValueChanged += (value) => SetImgEditorValues();
        _brightnessSlider.ValueChanged += (value) => SetImgEditorValues();
        _contrastSlider.ValueChanged += (value) => SetImgEditorValues();
        _outlineCheckbox.Toggled += (pressed) => SetImgEditorValues();
        _outlineThicknessSlider.ValueChanged += (value) => SetImgEditorValues();
        _outlineColorPicker.ColorChanged += (color) => SetImgEditorValues();
        _colorCountSpinBox.ValueChanged += OnColorReductionSpinBoxChanged;
        _colorReductionCheckbox.Toggled += (pressed) => SetImgEditorValues();

        _saveButton.Pressed += async () => await OnSaveButtonPressed();
        LoadExternalImg.Pressed += async () => await OnLoadExternalImgBtnPressed();

        GlobalEvents.Instance.OnPaletteChanged += OnPaletteChanged;
        GlobalEvents.Instance.OnEffectsChangesStarted += OnEffectsChangesStarted;
        GlobalEvents.Instance.OnEffectsChangesEnded += OnEffectsChangesEnded;
        UseExternalPaletteChkBtn.Toggled += OnUseExternalPaletteBtnToggled;
        _resetImgCorrectionBtn.Pressed += OnResetImgCorrectionBtnPressed;

        //PaletteLoaderPanel.Visible = false;
        _effectStatusLabel.Text = "Ready";
        EffectStatusMainPanel.Visible = false;
        UpdateUIElementsOnLoad();
        _isFirstRun = false;

        UpdateViewPortTextures(_originalImage);
        SetImgEditorValues();

    }

    private void OnResetImgCorrectionBtnPressed()
    {
        UpdateUIElementsOnLoad(true);
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

        SetImgEditorValues();
        UpdateUIElementsOnLoad();

    }

    public async Task LoadTextureFromImgFile(string path)
    {
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

        // Create the ImageTexture and update ViewPorts
        UpdateViewPortTextures(imgToLoad);

    }

    public void UpdateViewPortTextures(Image imageToLoad)
    {
        ImageTexture texture = ImageTexture.CreateFromImage(imageToLoad);

        if (_ImgEditor.HDTextureRect != null && _ImgEditor.ImgTextRect != null)
        {
            _ImgEditor.ImgTextRect.Texture = texture;
            _ImgEditor.HDTextureRect.Texture = texture;
            _ImgEditor.HDSubviewPort.Size = imageToLoad.GetSize();
            // Ensure GetUniqueColorsCount is not blocking the main thread.
            //_ImgEditor.NumColors = _ImgEditor.GetUniqueColorsCount(imgToLoad).Result;



            //var originalColorsList = _ImgEditor.GetColorFrequencies(imageToLoad).Keys.ToList();

            var originalColorsList = _ImgEditor.GetColorFrequencies(imageToLoad);
            int colorsCount = originalColorsList.Count;

            GD.PrintT("## Loaded IMG -> Original Colors Count = " + colorsCount);

            List<Color> originalColorsListSorted = originalColorsList.OrderByDescending(pair => pair.Value).Select(pair => pair.Key).ToList();

            // int colorsCount = originalColorsList.Count;
            //int colorsCount = Task.Run(() => _ImgEditor.GetColorFrequencies(imageToLoad).Count).Result;


            _ImgEditor.NumColors = colorsCount;

            _colorCountSpinBox.MaxValue = colorsCount;
            _colorCountSpinBox.Value = _ImgEditor.NumColors;

            //These two values below cannot be changed anywhere else ever.. They are always set at the start or at new Img Load only
            _ImgEditor.MaxNumColors = colorsCount;
            _ImgEditor.OriginalNumColors = colorsCount;
            _ImgEditor.OriginalImgPalette = GlobalUtil.GetGodotArrayFromList(originalColorsListSorted);
        }
        else
        {
            GD.PrintErr("_ImgEditor or _ImgEditor.ImgTextRect is null!");  // Debugging: Check initialization
        }

    }

    public void UpdateUIElementsOnLoad(bool isReset = false)
    {
        if (_ImgEditor.ImgTextRect.Texture != null)
        {
            _originalTexture = _ImgEditor.ImgTextRect.Texture;
            _originalImage = _originalTexture.GetImage();
        }
        //_colorCountSpinBox.MaxValue = _ImgEditor.NumColors; //Mathf.Clamp(value, min, max);

        _colorCountSpinBox.MaxValue = Mathf.Clamp(_ImgEditor.NumColors, 1, _ImgEditor.MAX_PALETTE_SIZE);
        _colorCountSpinBox.Value = _ImgEditor.NumColors;
        _brightnessSlider.Value = 0.0f;
        _brightnessValueLabel.Text = _brightnessSlider.Value.ToString("0.0");
        _saturationSlider.Value = 1.0f;
        _saturationValueLabel.Text = _saturationSlider.Value.ToString("0.0");
        _contrastSlider.Value = 1.0f;
        _contrastValueLabel.Text = _contrastSlider.Value.ToString("0.0");
        _outlineLabel.Text = _outlineThicknessSlider.Value.ToString("0.0");
        _outlineCheckbox.ButtonPressed = false;
        _outlineThicknessSlider.Value = 0;
        _outlineColorPicker.Color = Colors.Black;
        _colorReductionCheckbox.ButtonPressed = false;

        if (isReset)
        {
            _colorCountSpinBox.MaxValue = _ImgEditor.OriginalNumColors;
            _colorCountSpinBox.Value = _ImgEditor.OriginalNumColors;
            _ImgEditor.NumColors = _ImgEditor.OriginalNumColors;
        }

    }

    private async void SetImgEditorValues()
    {
        if (_isFirstRun) return;

        GlobalEvents.Instance.OnEffectsChangesStarted.Invoke(this.Name);

        //GET UI VALUES AND SET TO THE IMAGE EDITOR
        _ImgEditor.EnableColorReduction = _colorReductionCheckbox?.ButtonPressed ?? false;
        _useExternalPalette = UseExternalPaletteChkBtn?.ButtonPressed ?? false;
        _ImgEditor._useExternalPalette = _useExternalPalette;
        _ImgEditor.SaturationValue = (float)(_saturationSlider?.Value ?? 1.0f);
        _saturationValueLabel.Text = _saturationSlider.Value.ToString("0.0");
        _ImgEditor.BrightnessValue = (float)(_brightnessSlider?.Value ?? 0.0f);
        _brightnessValueLabel.Text = _brightnessSlider.Value.ToString("0.0");
        _ImgEditor.ConstrastValue = (float)(_contrastSlider?.Value ?? 1.0f);
        _contrastValueLabel.Text = _contrastSlider.Value.ToString("0.0");

        //SET THE COLOR COUNT FOR IMG EDITOR AND COLOR REDUCTION SPINBOX
        int totalColors = 0;

        if (_ImgEditor.EnableColorReduction == false)
        {
            //If no color reduction, reset image count values to it's original.
            totalColors = _ImgEditor.OriginalNumColors;
        }
        else
        {
            //If WE DO HAVE color reduction, we also count the Persistent Colors + whatever is the user input in the spinbox
            totalColors = PaletteLoaderPanel.PersistPaletteColors.Count + (int)(_colorCountSpinBox?.Value ?? 0);
        }
        _colorCountSpinBox.MaxValue = totalColors;
        //_colorCountSpinBox.Value = totalColors;
        _ImgEditor.NumColors = totalColors;


        //FOR NON_EXTERNAL PALETTE -> APPLY LOGIC TO GET THE PALETTE COLORS FROM ORIGINAL IMAGE + PERSISTENT COLORS
        if (!_useExternalPalette)
        {
            int paletteSize = _ImgEditor.NumColors - PaletteLoaderPanel.PersistPaletteColors.Count;
            if (paletteSize <= 0) paletteSize = _ImgEditor.NumColors;//Safety net code line to not have it as Zero. 

            _currentPaletteColors = await _ImgEditor.GetNewColorPalette(paletteSize);
        }
        //FOR EXTERNAL PALETTE WE ARE SETTING THIS IN THE PALETTE LOADER PANEL - SEE METHOD OnPaletteChanged();

        //FINAL UPDATE OF THE UI ELEMENTS + SET THE SHADER PARAMETERS and SHADER PALETTE
        _ImgEditor.ShaderPalette = GlobalUtil.GetGodotArrayFromList(PaletteLoaderPanel.PersistPaletteColors) + _currentPaletteColors;
        _ImgEditor.UpdateShaderParameters();
        PaletteLoaderPanel.UpdatePaletteListGrid(_currentPaletteColors);

        GD.PrintT("SetImgEditorValues -> Total colors = " + _ImgEditor.ShaderPalette.Count
        + " Persist Colors = " + PaletteLoaderPanel.PersistPaletteColors.Count
         + " Current Img Colors = " + _currentPaletteColors.Count);

        GlobalEvents.Instance.OnEffectsChangesEnded.Invoke(this.Name, _currentPaletteColors);
    }

    private void OnPaletteChanged(Godot.Collections.Array<Color> list)
    {
        GD.Print("Palette changed to # : " + list.Count);
        //int colorCount = list.Count;

        _ImgEditor.MaxNumColors = list.Count;
        _ImgEditor.NumColors = list.Count;

        _colorCountSpinBox.MaxValue = list.Count;
        //_colorCountSpinBox.Value = _ImgEditor.NumColors;

        _currentPaletteColors = list;

        SetImgEditorValues();
    }

    private void OnUseExternalPaletteBtnToggled(bool toggledOn)
    {
        UseExternalPaletteChkBtn.Text = UseExternalPaletteChkBtn.ButtonPressed.ToString();
        SetImgEditorValues();
    }

    private void OnColorReductionSpinBoxChanged(double value)
    {
        if (_isFirstRun)
            return;

        if (_colorReductionCheckbox.ButtonPressed == true)
        {
            SetImgEditorValues();
        }
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

    // private async Task ApplyEffectsAsync(bool doColorReduction, int colorCount, bool doOutline,
    //     int outlineThickness, Color outlineColor, bool doDithering, float ditheringStrength)
    // {

    //     //I always start with a copy of the original image, preserving the original for "undo" (to be implemented)
    //     Image modifiedImage = (Image)_originalImage.Duplicate(); //First code version (working)

    //     if (doOutline)
    //     {
    //         modifiedImage = await Task.Run(() => AddOutline(modifiedImage, outlineThickness, outlineColor));
    //     }

    //     // Update the texture *after* other effects have been applied
    //     CallDeferred(nameof(UpdateTexture), modifiedImage);
    // }

    // private Image AddOutline(Image image, int thickness, Color color)
    // {
    //     int width = image.GetWidth();
    //     int height = image.GetHeight();
    //     byte[] originalData = image.GetData();
    //     byte[] outlinedData = new byte[originalData.Length];
    //     Array.Copy(originalData, outlinedData, originalData.Length);

    //     byte outlineR = (byte)(color.R * 255);
    //     byte outlineG = (byte)(color.G * 255);
    //     byte outlineB = (byte)(color.B * 255);
    //     byte outlineA = (byte)(color.A * 255);

    //     Parallel.For(0, height, y =>
    //     {
    //         for (int x = 0; x < width; x++)
    //         {
    //             int originalIndex = (y * width + x) * 4;
    //             if (originalData[originalIndex + 3] == 0) continue;
    //             for (int oy = Math.Max(0, y - thickness); oy <= Math.Min(height - 1, y + thickness); oy++)
    //             {
    //                 for (int ox = Math.Max(0, x - thickness); ox <= Math.Min(width - 1, x + thickness); ox++)
    //                 {
    //                     if (ox == x && oy == y) continue;
    //                     int outlinedIndex = (oy * width + ox) * 4;
    //                     if (outlinedData[outlinedIndex + 3] == 0)
    //                     {
    //                         outlinedData[outlinedIndex + 0] = outlineR;
    //                         outlinedData[outlinedIndex + 1] = outlineG;
    //                         outlinedData[outlinedIndex + 2] = outlineB;
    //                         outlinedData[outlinedIndex + 3] = outlineA;
    //                     }
    //                 }
    //             }
    //         }
    //     });

    //     return Image.CreateFromData(width, height, false, Image.Format.Rgba8, outlinedData);
    // }


    private async Task OnSaveButtonPressed()
    {
        //LOGIC TO GET MODIFIED IMAGE
        if (_ImgEditor.ImgTextRect.Texture == null) return;

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        Texture2D texture = (Texture2D)_ImgEditor.HDSubviewPort.GetTexture();
        Image modifiedImage = (Image)texture.GetImage();
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        //OPEN DIALOG TO SAVE TO A PATH
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

    }

    public void OnSaveData(SaveGameData newSaveGameData)
    {
        GD.PrintT("Started OnSaveData from:", this.Name);
        newSaveGameData.SaturationSliderValue = (float)_saturationSlider.Value;
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

        _saturationSlider.Value = newLoadData.SaturationSliderValue;
        _brightnessSlider.Value = newLoadData.BrightnessSliderValue;
        _outlineCheckbox.ButtonPressed = newLoadData.OutlineIsOn;
        _outlineThicknessSlider.Value = newLoadData.OutlineThicknessSliderValue;
        _outlineColorPicker.Color = newLoadData.OutlineColor;
        _colorReductionCheckbox.ButtonPressed = newLoadData.ColorReductionIsOn;
        _colorCountSpinBox.Value = newLoadData.ColorReductionValue;

        SetImgEditorValues();

        // if (newLoadData.OutlineIsOn)
        // {
        //     await ApplyEffectsAsync(
        //         newLoadData.ColorReductionIsOn,
        //         (int)newLoadData.ColorReductionValue,
        //         newLoadData.OutlineIsOn,
        //         (int)newLoadData.OutlineThicknessSliderValue,
        //         newLoadData.OutlineColor,
        //         false,
        //         0);
        // }

    }

    public override void _ExitTree()
    {
        GlobalEvents.Instance.OnPaletteChanged -= OnPaletteChanged;
    }


}
