
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public partial class MainInterfaceUI : Node
{

    [Export] private Button _saveConfigBtn;
    [Export] private OptionButton _presetListOptBtn;
    [Export] private Button _loadConfigBtn;

    //Main Settings and Folder Path Variables
    [Export] private Button _selectFolderPathBtn;
    [Export] private Button _openFolderPathBtn;
    [Export] private LineEdit _spriteGenFolderPathLineEdit;
    [Export] private MarginContainer _settingsMainPanel;
    //[Export] private Button _openSettingPanelBtn;
    [Export] private Button _testConfigBtn;



    public override void _Ready()
    {

        _testConfigBtn.Pressed += () => LoadPresetButtonItems();
        //Connect UI Signals
        _saveConfigBtn.Pressed += OnSaveConfigBtnPressed;
        _loadConfigBtn.Pressed += OnLoadConfigBtnPressed;
        _presetListOptBtn.ItemSelected += async (long index) => await OnPresetSelected(index);
        _spriteGenFolderPathLineEdit.TextChanged += (newDir) => GlobalUtil.OnFolderSelected(newDir, _spriteGenFolderPathLineEdit);
        _selectFolderPathBtn.Pressed += OnSelectFolderPathPressed;
        _openFolderPathBtn.Pressed += OnOpenFolderPathPressed;
        //_openSettingPanelBtn.Pressed += OnOpenSettingsPanelBtnPressed;
        //_settingsMainPanel.Visible = false;
        _spriteGenFolderPathLineEdit.Text = GlobalUtil.SaveFolderPath;

        GlobalUtil.SaveFolderPath = ProjectSettings.GlobalizePath(Const.SAVE_GAME_PATH);
        _spriteGenFolderPathLineEdit.Text = GlobalUtil.SaveFolderPath;

        LoadPresetButtonItems();

    }

    private async Task OnPresetSelected(long index)
    {
        string selectedItemName = _presetListOptBtn.GetItemText((int)index);
        if (selectedItemName == "None") return;

        // SaveGameData saveGameData = GD.Load<SaveGameData>(Const.PRESET_SAVEDATA_FOLDER_PATH + selectedItemName);

        if (!string.IsNullOrEmpty(selectedItemName))
        {
            // Log.Debug("Preset Selected: " + selectedItemName);

            string fullLoadFilePath = Const.PRESET_SAVEDATA_FOLDER_PATH + selectedItemName;
            await SaveGameManager.Instance.LoadGameDataFromPath(fullLoadFilePath);
        }

    }

    private void LoadPresetButtonItems()
    {
        var files = DirAccess.GetFilesAt(Const.PRESET_SAVEDATA_FOLDER_PATH);
        _presetListOptBtn.Clear();
        _presetListOptBtn.AddItem(Const.PRESET_NONE_FILE_PATH.GetFile());

        foreach (string file in files)
        {
            if (file.EndsWith(".res") || file.EndsWith(".tres"))
            {
                //Don't add the none file again as we are manully adding it to position 0 above.
                if (file == Const.PRESET_NONE_FILE_PATH.GetFile()) continue;

                SaveGameData presetSaveGameData = new();
                try
                {
                    //Just a safety check to ensur eonly SaveGameData files are loaded to the button
                    presetSaveGameData = GD.Load<SaveGameData>(Const.PRESET_SAVEDATA_FOLDER_PATH + file);
                    if (presetSaveGameData != null)
                    {
                        _presetListOptBtn.AddItem(file);
                    }

                }
                catch (System.Exception error)
                {
                    Log.Error("Failed to load resource as SaveGameData: " + file + "  -> Error:" + error.Message);
                }

            }
        }

        _presetListOptBtn.Select(0);
    }


    // private void OnOpenSettingsPanelBtnPressed()
    // {
    //     _settingsMainPanel.Visible = !_settingsMainPanel.Visible;
    //     _openSettingPanelBtn.Text = _settingsMainPanel.Visible ? "Close Settings" : "Main Settings";
    // }


    private async void OnLoadConfigBtnPressed()
    {
        using Godot.FileDialog fileDialog = new Godot.FileDialog
        {
            FileMode = FileDialog.FileModeEnum.OpenFile,
            Filters = new string[] { "*.tres ; SaveGame File" },
            Access = FileDialog.AccessEnum.Filesystem
        };

        AddChild(fileDialog);

        fileDialog.CurrentDir = ProjectSettings.GlobalizePath(Const.SAVE_GAME_PATH); //Set this after adding Child to Scene

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        fileDialog.PopupCentered();

        await ToSignal(fileDialog, FileDialog.SignalName.FileSelected);

        string file = fileDialog.CurrentFile;
        string fullLoadFilePath = fileDialog.CurrentDir + "/" + file;

        await SaveGameManager.Instance.LoadGameDataFromPath(fullLoadFilePath);

    }

    private async void OnSaveConfigBtnPressed()
    {
        using FileDialog fileDialog = new FileDialog
        {
            FileMode = FileDialog.FileModeEnum.SaveFile,
            Filters = new string[] { "*.tres ; SaveGame File" },
            Access = FileDialog.AccessEnum.Filesystem
        };

        AddChild(fileDialog); // Add to scene first 

        //string folderCurrentDir = Const.USER_ROOT_FOLDER_PATH; // Ensure it's inside user:// or res://
        string globalizedSavePath = ProjectSettings.GlobalizePath(Const.SAVE_GAME_PATH);

        if (!GlobalUtil.HasDirectory(globalizedSavePath, this).Result)
        {
            //Log.Debug("Directory does NOT exist: " + globalizedSavePath);
            globalizedSavePath = "user://"; // Fallback to a safe default
        }

        fileDialog.CurrentDir = globalizedSavePath; //Set Current Directory at the end after adding Child to Scene otherwise it was not working

        fileDialog.FileSelected += async (path) => await SaveGameManager.Instance.SaveGameData(path);

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        fileDialog.PopupCentered(); // Show the dialog

        //await SaveGameManager.Instance.SaveGameData(saveFileName);
    }

    private void OnOpenFolderPathPressed()
    {

        string directory = ProjectSettings.GlobalizePath(_spriteGenFolderPathLineEdit.Text);
        if (GlobalUtil.HasDirectory(directory, this).Result)
        {
            OS.ShellOpen(directory);
        }
        else
        {
            Log.Error("Directory does not exist: " + directory);

            using Godot.AcceptDialog acceptDialog = new Godot.AcceptDialog
            {
                Title = "Error: Directory not Found",
                DialogText = "Directory does not exist: " + directory
            };

            AddChild(acceptDialog);
            acceptDialog.PopupCentered();
        }
    }

    private void OnSelectFolderPathPressed()
    {
        using Godot.FileDialog fileDialog = new Godot.FileDialog
        {
            FileMode = FileDialog.FileModeEnum.OpenDir,
            Access = FileDialog.AccessEnum.Filesystem
        };

        AddChild(fileDialog);

        fileDialog.CurrentDir = GlobalUtil.SaveFolderPath; //Set this after adding Child to Scene

        fileDialog.DirSelected += (newDir) => GlobalUtil.OnFolderSelected(newDir, _spriteGenFolderPathLineEdit);

        fileDialog.PopupCentered();

    }

}
