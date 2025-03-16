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
    [Export] public PaletteListGrid PaletteListGridContainer;

    public Godot.Collections.Array<Color> LoadedPaletteColors = new();

    private const int MAX_PALETTE_SIZE = 256;

    private string currentHextFileText = "";



    public override void _Ready()
    {
        LoadExtPaletteBtn.Pressed += async () => await OnLoadExtPaletteBtnPressed();
        LoadedPaletteColors.Add(Colors.Black);
    }

    private async Task OnLoadExtPaletteBtnPressed()
    {
        currentHextFileText = "";
        LoadedPaletteColors.Clear();
        PaletteListGridContainer.ClearGridList();

        await LoadTextFromHexFile(); // We load the result from this into currentHextFileText;
        LoadedPaletteColors = GetHexFileColors(currentHextFileText);

        GlobalEvents.Instance.OnPaletteChanged.Invoke(LoadedPaletteColors);
        UpdatePaletteListGrid(LoadedPaletteColors);

    }

    public void UpdatePaletteListGrid(Godot.Collections.Array<Color> paletteColors)
    {
        PaletteListGridContainer.ClearGridList();
        foreach (var color in paletteColors)
        {
            PaletteListGridContainer.AddGridItem(color);
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
