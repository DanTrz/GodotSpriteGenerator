using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public partial class PaletteLoader : MarginContainer
{

    [Export] public Button LoadExtPaletteBtn;

    [Export] public Button AddPersistColorsBtn;

    [Export] public Button ClearPersistColorsBtn;

    [Export] public ColorPickerButton PersistColorPickerBtn;
    [Export] public PaletteListFlowGrid ExtPaletteListFlowGrid;
    [Export] public PaletteListFlowGrid PersistColorListFlowGrid;
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

        ExternalPaletteColors.Clear();
        CombinedPaletteColors.Clear();
        PersistPaletteColors.Clear();

        OnUseExternalPaletteCheckBtnPressed();
    }

    private void OnUseExternalPaletteCheckBtnPressed()
    {
        LoadExtPaletteBtn.Visible = UseExternalPaletteCheckBtn.ButtonPressed;
    }


    private void OnClearPersistColorsBtnPressed()
    {
        PersistColorListFlowGrid.ClearFlowList();
        PersistPaletteColors.Clear();
        CombinedPaletteColors.Clear();
        CombinedPaletteColors = ExternalPaletteColors;
        Callable.From(() => GlobalEvents.Instance.OnPaletteChanged?.Invoke(CombinedPaletteColors)).CallDeferred();
    }

    private async Task OnAddPersistColorsBtnPressed()
    {
        PersistPaletteColors.Add(PersistColorPickerBtn.Color);
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        var godotPalette = GlobalUtil.GetGodotArrayFromColorList(PersistPaletteColors);
        UpdatePaletteFlowList(godotPalette, PersistColorListFlowGrid);
    }

    private async Task OnLoadExtPaletteBtnPressed()
    {
        currentHextFileText = "";
        ExternalPaletteColors.Clear();
        CombinedPaletteColors.Clear();

        ExtPaletteListFlowGrid.ClearFlowList();

        await LoadTextFromHexFile(); // We load the result from this into currentHextFileText;
        ExternalPaletteColors = GetHexFileColors(currentHextFileText);

        var godotPersistPalette = GlobalUtil.GetGodotArrayFromColorList(PersistPaletteColors);
        CombinedPaletteColors = ExternalPaletteColors + godotPersistPalette;

        GlobalUtil.PrintActionTargetListeners(GlobalEvents.Instance.OnPaletteChanged, "Global OnPaletteChanged");
        GlobalEvents.Instance.OnPaletteChanged?.Invoke(CombinedPaletteColors);

        UpdatePaletteFlowList(ExternalPaletteColors, ExtPaletteListFlowGrid);
    }

    public void UpdatePaletteFlowList(Godot.Collections.Array<Color> paletteColors, PaletteListFlowGrid paletteFlowListGrid = null)
    {
        if (paletteFlowListGrid == null)
        {
            paletteFlowListGrid = ExtPaletteListFlowGrid;
            ExternalPaletteColors = paletteColors;
        }

        paletteFlowListGrid.ClearFlowList();

        foreach (var color in paletteColors)
        {
            paletteFlowListGrid.AddFlowGridItem(color);
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
            Log.Error("File is not a HEX File: " + selectedFiled);
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
            Log.Error("Hex file cannot have more than " + MAX_PALETTE_SIZE + " colors. Hex File color count =  " + colorCount);
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
