using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Godot;
public partial class PaletteLoader : MarginContainer
{

    // PALETTE OPTIONS ITEMS

    [Export] public Button LoadExtPaletteBtn;

    [Export] public Button AddPersistColorsBtn;

    [Export] public Button ClearPersistColorsBtn;

    [Export] public ColorPickerButton PersistColorPickerBtn;
    [Export] public PaletteListGrid ExtPaletteListGridContainer;

    [Export] public PaletteListGrid PersistColorListGridContainer;

    [Export] public CheckButton UseExternalPaletteCheckBtn;

    public Godot.Collections.Array<Color> ExternalPaletteColors = new();

    public List<Color> PersistPaletteColors = new();

    public Godot.Collections.Array<Color> CombinedPaletteColors = new();

    private const int MAX_PALETTE_SIZE = 256;

    private string currentHextFileText = "";



    public override void _Ready()
    {
        LoadExtPaletteBtn.Pressed += async () => await OnLoadExtPaletteBtnPressed();
        AddPersistColorsBtn.Pressed += async () => await OnAddPersistColorsBtnPressed();
        UseExternalPaletteCheckBtn.Pressed += OnUseExternalPaletteCheckBtnPressed;
        ClearPersistColorsBtn.Pressed += OnClearPersistColorsBtnPressed;


        //ExternalPaletteColors.Add(Colors.Black);
        ExternalPaletteColors.Clear();
        CombinedPaletteColors.Clear();
        PersistPaletteColors.Clear();

        OnUseExternalPaletteCheckBtnPressed();

        //OnClearPersistColorsBtnPressed();
    }

    private void OnUseExternalPaletteCheckBtnPressed()
    {
        LoadExtPaletteBtn.Visible = UseExternalPaletteCheckBtn.ButtonPressed;
    }


    private void OnClearPersistColorsBtnPressed()
    {
        PersistColorListGridContainer.ClearGridList();
        PersistPaletteColors.Clear();
        CombinedPaletteColors.Clear();

        CombinedPaletteColors = ExternalPaletteColors;

        Callable.From(() => GlobalEvents.Instance.OnPaletteChanged?.Invoke(CombinedPaletteColors)).CallDeferred();

        //GlobalEvents.Instance.OnPaletteChanged.Invoke(CombinedPaletteColors);
    }


    private async Task OnAddPersistColorsBtnPressed()
    {

        PersistPaletteColors.Add(PersistColorPickerBtn.Color);

        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);

        //CombinedPaletteColors.Clear();
        //CombinedPaletteColors = ExternalPaletteColors + PersistPaletteColors;

        //GlobalEvents.Instance.OnPaletteChanged.Invoke(CombinedPaletteColors);

        var godotPalette = GlobalUtil.GetGodotArrayFromList(PersistPaletteColors);
        UpdatePaletteListGrid(godotPalette, PersistColorListGridContainer);

        //UpdatePaletteListGrid(CombinedPaletteColors, ExtPaletteListGridContainer);

    }


    private async Task OnLoadExtPaletteBtnPressed()
    {
        currentHextFileText = "";
        ExternalPaletteColors.Clear();
        CombinedPaletteColors.Clear();

        ExtPaletteListGridContainer.ClearGridList();

        await LoadTextFromHexFile(); // We load the result from this into currentHextFileText;
        ExternalPaletteColors = GetHexFileColors(currentHextFileText);

        var godotPersistPalette = GlobalUtil.GetGodotArrayFromList(PersistPaletteColors);
        CombinedPaletteColors = ExternalPaletteColors + godotPersistPalette;

        GlobalUtil.PrintActionTargetListeners(GlobalEvents.Instance.OnPaletteChanged, "Global OnPaletteChanged");
        GlobalEvents.Instance.OnPaletteChanged?.Invoke(CombinedPaletteColors);

        UpdatePaletteListGrid(ExternalPaletteColors, ExtPaletteListGridContainer);

    }

    // private void SendPaletteChangedEvent(Godot.Collections.Array<Color> paletteColors)
    // {
    //     GlobalEvents.Instance.OnPaletteChanged.Invoke(paletteColors);
    // }

    public void UpdatePaletteListGrid(Godot.Collections.Array<Color> paletteColors, PaletteListGrid paletteListGrid = null)
    {
        if (paletteListGrid == null)
        {
            paletteListGrid = ExtPaletteListGridContainer;
            ExternalPaletteColors = paletteColors;
        }

        paletteListGrid.ClearGridList();

        foreach (var color in paletteColors)
        {
            paletteListGrid.AddGridItem(color);
        }
    }

    private async Task LoadTextFromHexFile()
    {
        using Godot.FileDialog fileDialog = new Godot.FileDialog
        {
            FileMode = FileDialog.FileModeEnum.OpenFile,
            Filters = new string[] { "*.hex, *.HEX ; HEX Files" },
            Access = FileDialog.AccessEnum.Filesystem
        };

        //CallDeferred(MethodName.AddChild, fileDialog);
        AddChild(fileDialog);

        fileDialog.CurrentDir = GlobalUtil.SaveFolderPath; //Set this after adding Child to Scene

        fileDialog.PopupCentered();

        await ToSignal(fileDialog, FileDialog.SignalName.FileSelected);

        string selectedFiled = fileDialog.CurrentDir + "/" + fileDialog.CurrentFile;

        if (selectedFiled.EndsWith(".hex") == false)
        {
            GD.PrintErr("File is not a HEX File: " + selectedFiled);
        }

        string fileGlobalPath = ProjectSettings.GlobalizePath(selectedFiled);

        string fileText = File.ReadAllText(fileGlobalPath);

        currentHextFileText = fileText;

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

    }


    private Godot.Collections.Array<Color> GetHexFileColors(string hexFileText)
    {
        string[] lines = hexFileText.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
        int colorCount = lines.Count() - 1;
        Godot.Collections.Array<Color> paletteColors = new();

        if (colorCount > MAX_PALETTE_SIZE)
        {
            GD.PrintErr("Hex file cannot have more than " + MAX_PALETTE_SIZE + " colors. Hex File color count =  " + colorCount);
        }

        foreach (var hexColorText in lines)
        {

            if (string.IsNullOrEmpty(hexColorText)) continue;

            Color newColor = new Color(hexColorText.Trim());
            paletteColors.Add(newColor);
        }

        return paletteColors;
    }
}
