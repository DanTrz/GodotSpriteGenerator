using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Image = Godot.Image;

public partial class ImageEditorMainPanel : PanelContainer
{
    [Export] public ImageEditor ImgEditorCore;
    [Export] public SliderValueBox SaturationSlider;
    [Export] public SliderValueBox BrightnessSlider;
    [Export] public SliderValueBox ContrastSlider;
    [Export] private Button _resetImgCorrectionBtn;
    [Export] public SliderValueBox Outline2DSlider;
    [Export] public ColorPickerButton Outline2DColorPicker;
    [Export] public SliderValueBox Inline2DSlider;
    [Export] public ColorPickerButton Inline2DColorPicker;
    [Export] public CheckBox ColorReductionCheckbox;
    [Export] public SpinBox ColorCountSpinBox;
    [Export] private Button _saveButton;
    [Export] private Label _effectStatusLabel;
    [Export] private MarginContainer _effectStatusMainPanel;

    //[Export] public CheckButton UseExternalPaletteChkBtn;
    [Export] public PaletteLoader PaletteLoaderFlow;

    [Export] public Button LoadExternalImg;

    [Export] public Label PaletteSizeMaxValueLbl;

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
        if (ImgEditorCore.ImgTextRect.Texture != null)
        {
            _originalTexture = ImgEditorCore.ImgTextRect.Texture;
            _originalImage = _originalTexture.GetImage();
        }

        //Connect UI Sigansl for most of the effects
        SaturationSlider.ValueChanged += (value) => SetImgEditorValues();
        BrightnessSlider.ValueChanged += (value) => SetImgEditorValues();
        ContrastSlider.ValueChanged += (value) => SetImgEditorValues();
        Outline2DSlider.ValueChanged += (value) => SetImgEditorValues();
        Inline2DSlider.ValueChanged += (value) => SetImgEditorValues();
        Outline2DColorPicker.ColorChanged += (color) => SetImgEditorValues();
        Inline2DColorPicker.ColorChanged += (color) => SetImgEditorValues();
        ColorCountSpinBox.ValueChanged += OnColorReductionSpinBoxChanged;
        ColorReductionCheckbox.Toggled += (pressed) => SetImgEditorValues();

        _saveButton.Pressed += async () => await OnSaveButtonPressed();
        LoadExternalImg.Pressed += async () => await OnLoadExternalImgBtnPressed();

        GlobalEvents.Instance.OnPaletteChanged += OnPaletteChangedInImageEditor;
        GlobalEvents.Instance.OnEffectsChangesStarted += OnEffectsChangesStarted;
        GlobalEvents.Instance.OnEffectsChangesEnded += OnEffectsChangesEnded;
        GlobalEvents.Instance.OnForcedPaletteSize += (value) => PaletteSizeMaxValueLbl.Text = value.ToString();
        GlobalEvents.Instance.OnSpriteSheetCreated += async (image) => await LoadTextureFromImg(image);

        //UseExternalPaletteChkBtn.Toggled += OnUseExternalPaletteBtnToggled;
        _resetImgCorrectionBtn.Pressed += OnResetImgCorrectionBtnPressed;



        //PaletteLoaderPanel.Visible = false;
        _effectStatusLabel.Text = "Ready";
        _effectStatusMainPanel.Visible = false;
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
        await LoadTextureFromImg(selectedFiled);
    }

    public async Task LoadTextureFromImg(string path)
    {
        Image imgToLoad = Image.LoadFromFile(path);
        await ProcessImageAndReloadViewPorts(imgToLoad);
    }

    public async Task LoadTextureFromImg(Image imageToLoad)
    {
        Image imgToLoad = imageToLoad;
        await ProcessImageAndReloadViewPorts(imgToLoad);
    }

    private async Task ProcessImageAndReloadViewPorts(Image imgToLoad)
    {
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
        SetImgEditorValues();
        UpdateUIElementsOnLoad();
    }

    public void UpdateViewPortTextures(Image imageToLoad)
    {
        ImageTexture texture = ImageTexture.CreateFromImage(imageToLoad);

        if (ImgEditorCore.HDTextureRect != null && ImgEditorCore.ImgTextRect != null)
        {
            ImgEditorCore.ImgTextRect.Texture = texture;
            ImgEditorCore.HDTextureRect.Texture = texture;
            ImgEditorCore.HDSubviewPort.Size = imageToLoad.GetSize();
            //_ImgEditor.HDTextureRect.Size = imageToLoad.GetSize();
            // Ensure GetUniqueColorsCount is not blocking the main thread.
            //_ImgEditor.NumColors = _ImgEditor.GetUniqueColorsCount(imgToLoad).Result;



            //var originalColorsList = _ImgEditor.GetColorFrequencies(imageToLoad).Keys.ToList();

            var originalColorsList = ImgEditorCore.GetColorFrequencies(imageToLoad);
            int colorsCount = originalColorsList.Count;

            //Log.Debug("## Loaded IMG -> Original Colors Count = " + colorsCount);

            List<Color> originalColorsListSorted = originalColorsList.OrderByDescending(pair => pair.Value).Select(pair => pair.Key).ToList();

            // int colorsCount = originalColorsList.Count;
            //int colorsCount = Task.Run(() => _ImgEditor.GetColorFrequencies(imageToLoad).Count).Result;


            ImgEditorCore.NumColorsLocal = colorsCount;

            //_colorCountSpinBox.MaxValue = colorsCount;
            ColorCountSpinBox.MaxValue = colorsCount;
            ColorCountSpinBox.Value = ImgEditorCore.NumColorsLocal;

            //These two values below cannot be changed anywhere else ever.. They are always set at the start or at new Img Load only
            ImgEditorCore.MaxNumColors = colorsCount;
            ImgEditorCore.OriginalNumColors = colorsCount;
            PaletteSizeMaxValueLbl.Text = colorsCount.ToString();
            ImgEditorCore.OriginalImgPalette = GlobalUtil.GetGodotArrayFromColorList(originalColorsListSorted);
        }
        else
        {
            Log.Error("_ImgEditor or _ImgEditor.ImgTextRect is null!");  // Debugging: Check initialization
        }

    }

    public void UpdateUIElementsOnLoad(bool isReset = false)
    {
        if (ImgEditorCore.ImgTextRect.Texture != null)
        {
            _originalTexture = ImgEditorCore.ImgTextRect.Texture;
            _originalImage = _originalTexture.GetImage();
        }
        //_colorCountSpinBox.MaxValue = _ImgEditor.NumColors; //Mathf.Clamp(value, min, max);

        //_colorCountSpinBox.MaxValue = Mathf.Clamp(_ImgEditor.NumColors, 1, _ImgEditor.MAX_PALETTE_SIZE);
        ColorCountSpinBox.Value = ImgEditorCore.NumColorsLocal;
        BrightnessSlider.Value = 0.0f;
        SaturationSlider.Value = 1.0f;
        ContrastSlider.Value = 1.0f;
        Outline2DSlider.Value = 0;
        Inline2DSlider.Value = 0;
        Outline2DColorPicker.Color = Colors.Black;
        Inline2DColorPicker.Color = Colors.Black;
        ColorReductionCheckbox.ButtonPressed = false;


        if (isReset)
        {
            ColorCountSpinBox.MaxValue = ImgEditorCore.OriginalNumColors;
            ColorCountSpinBox.Value = ImgEditorCore.OriginalNumColors;
            ImgEditorCore.NumColorsLocal = ImgEditorCore.OriginalNumColors;
            _currentPaletteColors = ImgEditorCore.OriginalImgPalette;

            ImgEditorCore.ShaderPalette = ImgEditorCore.OriginalImgPalette;
            PaletteLoaderFlow.UseExternalPaletteCheckBtn.ButtonPressed = false;
            ImgEditorCore.UseExternalPalette = false;
            ImgEditorCore.UpdateShaderParameters();
            PaletteLoaderFlow.UpdatePaletteFlowList(ImgEditorCore.OriginalImgPalette);
        }

    }

    private async void SetImgEditorValues()
    {
        if (_isFirstRun) return;

        GlobalEvents.Instance.OnEffectsChangesStarted?.Invoke(this.Name);

        //GET UI VALUES AND SET TO THE IMAGE EDITOR
        ImgEditorCore.UseColorReduction = ColorReductionCheckbox?.ButtonPressed ?? false;
        _useExternalPalette = PaletteLoaderFlow.UseExternalPaletteCheckBtn?.ButtonPressed ?? false;
        ImgEditorCore.UseExternalPalette = _useExternalPalette;
        ImgEditorCore.SaturationValue = (float)(SaturationSlider?.Value ?? 1.0f);
        ImgEditorCore.BrightnessValue = (float)(BrightnessSlider?.Value ?? 0.0f);
        ImgEditorCore.ConstrastValue = (float)(ContrastSlider?.Value ?? 1.0f);
        ImgEditorCore.OutlineValue = (float)(Outline2DSlider?.Value ?? 1.0f);
        ImgEditorCore.InlineValue = (float)(Inline2DSlider?.Value ?? 1.0f);
        ImgEditorCore.OutlineColor = Outline2DColorPicker?.Color ?? Colors.Black;
        ImgEditorCore.InlineColor = Inline2DColorPicker?.Color ?? Colors.Black;


        //TODO this rest of this function needs a refactor - prone to ?//BUG
        // Issues may ocur as it's not clear when using ExternalPalette vs OriginalImgColor and both these with or without ColorReduction.  
        //SET THE COLOR COUNT FOR IMG EDITOR AND COLOR REDUCTION SPINBOX
        int totalColors = 0;

        if (ImgEditorCore.UseColorReduction == false && !_useExternalPalette)
        {
            //If no color reduction, reset image count values to it's original.
            totalColors = ImgEditorCore.OriginalNumColors;
        }
        else
        {
            //If WE DO HAVE color reduction, we also count the Persistent Colors + whatever is the user input in the spinbox
            totalColors = (PaletteLoaderFlow.PersistPaletteColors?.Count ?? 0) + (int)(ColorCountSpinBox?.Value ?? 0);
        }

        ImgEditorCore.NumColorsLocal = totalColors;

        //FOR NON_EXTERNAL PALETTE -> APPLY LOGIC TO GET THE PALETTE COLORS FROM ORIGINAL IMAGE + PERSISTENT COLORS
        if (!_useExternalPalette)
        {
            int persistentColorsCount = PaletteLoaderFlow.PersistPaletteColors?.Count ?? 0;

            int paletteSize = ImgEditorCore.NumColorsLocal - persistentColorsCount;
            if (paletteSize <= 0) paletteSize = ImgEditorCore.NumColorsLocal;//Safety net code line to not have it as Zero. 
            ImgEditorCore.PersistColorCount = persistentColorsCount;

            var tempPalette = await ImgEditorCore.GetNewColorPaletteKMeansClustering(paletteSize);
            if (tempPalette != null)
                _currentPaletteColors = tempPalette;
        }
        //FOR EXTERNAL PALETTE WE ARE SETTING THIS IN THE PALETTE LOADER PANEL - SEE METHOD OnPaletteChanged();

        //FINAL UPDATE OF THE UI ELEMENTS + SET THE SHADER PARAMETERS and SHADER PALETTE
        ImgEditorCore.ShaderPalette = GlobalUtil.GetGodotArrayFromColorList(PaletteLoaderFlow.PersistPaletteColors ?? new List<Color>()) + _currentPaletteColors;
        ImgEditorCore.UpdateShaderParameters();
        PaletteLoaderFlow.UpdatePaletteFlowList(_currentPaletteColors);

        // Log.Debug("SetImgEditorValues -> Total colors = " + ImgEditorCore.ShaderPalette?.Count
        // + " Persist Colors = " + PaletteLoader.PersistPaletteColors?.Count
        //  + " Current Img Colors = " + _currentPaletteColors?.Count);

        GlobalEvents.Instance.OnEffectsChangesEnded?.Invoke(this.Name, _currentPaletteColors);
    }

    private void OnPaletteChangedInImageEditor(Godot.Collections.Array<Color> externalPalette)
    {
        //Log.Debug("Palette changed to # : " + list.Count);
        ImgEditorCore.MaxNumColors = externalPalette.Count;
        ColorCountSpinBox.MaxValue = externalPalette.Count;
        ColorCountSpinBox.Value = externalPalette.Count;
        PaletteSizeMaxValueLbl.Text = externalPalette.Count.ToString();
        ImgEditorCore.NumColorsShaderValue = externalPalette.Count;
        ImgEditorCore.NumColorsLocal = externalPalette.Count;
        _currentPaletteColors = externalPalette;

        SetImgEditorValues();
    }



    private void OnColorReductionSpinBoxChanged(double value)
    {
        if (_isFirstRun)
            return;

        if (ColorReductionCheckbox.ButtonPressed == true)
        {
            SetImgEditorValues();
        }
    }

    private void OnEffectsChangesStarted(string fromNode)
    {
        //Log.Debug("Run: OnEffectsChangesStarted");
        _effectStatusLabel.Text = "Processing Image updates....";
        _effectStatusMainPanel.Visible = true;
    }

    private void OnEffectsChangesEnded(string fromNode, Godot.Collections.Array<Color> list)
    {
        //Log.Debug("Run: OnEffectsChangesEnded");
        _effectStatusLabel.Text = "Effects Applied !!! ";
        _effectStatusMainPanel.Visible = false;
        PaletteLoaderFlow.UpdatePaletteFlowList(list);
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
        if (ImgEditorCore.ImgTextRect.Texture == null) return;

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        Texture2D texture = (Texture2D)ImgEditorCore.HDSubviewPort.GetTexture();
        Image modifiedImage = (Image)texture.GetImage();
        //Log.Debug("Image prepared size:" + modifiedImage.GetSize());
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

        if (!GlobalUtil.HasDirectory(globalizedPath, this).Result)
        {
            //Log.Debug("Directory does NOT exist: " + folderCurrentDir);
            globalizedPath = "res://"; // Fallback to a safe default
        }
        fileDialog.CurrentDir = globalizedPath; //Set Current Directory at the end after adding Child to Scene otherwise it was not working
        fileDialog.FileSelected += (path) => SaveImageToFile(modifiedImage, path);
        fileDialog.PopupCentered(); // Show the dialog
    }

    private void OnFileSelected(string path)
    {
        //Log.Debug("Selected file path: " + path); //handle file selection
    }

    private void SaveImageToFile(Image image, string filePath)
    {
        Error err = image.SavePng(filePath);
        if (err != Error.Ok) Log.Error("Error saving image: " + err);
    }

    //Method used to update the TextureRect with the modified image
    private void UpdateTexture(Image modifiedImage)
    {
        ImgEditorCore.ImgTextRect.Texture = ImageTexture.CreateFromImage(modifiedImage);

        //-> Update shader params after image update with other C# code effects.
        ImgEditorCore.UpdateShaderParameters(); // Shader changes always last to ensure we will not get mixed up with other effects

        IsEffectProcessing = false;

    }

    public void OnSaveData(SaveGameData newSaveGameData)
    {

        //Log.Debug("Started OnSaveData from:", this.Name);
    }

    public void OnLoadData(SaveGameData newLoadData)
    {

        //Log.Debug("Started OnLoadData from:", this.Name);
        //UpdateUIElementsOnLoad();
        UpdateTexture(_originalImage);
        SetImgEditorValues();

    }

    public override void _ExitTree()
    {
        GlobalEvents.Instance.OnPaletteChanged -= OnPaletteChangedInImageEditor;
    }


}
