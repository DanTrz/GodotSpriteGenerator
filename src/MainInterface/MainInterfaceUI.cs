
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public partial class MainInterfaceUI : Node
{

    [Export] public Button _saveConfigBtn;
    [Export] public OptionButton SavedConfigListOptBtn;
    [Export] public Button _loadConfigBtn;

    //Main Settings and Folder Path Variables
    [Export] public Button _selectFolderPathBtn;
    [Export] public Button _openFolderPathBtn;
    [Export] public LineEdit _spriteGenFolderPathLineEdit;
    [Export] public MarginContainer _settingsMainPanel;
    [Export] public Button _openSettingPanelBtn;

    public override void _Ready()
    {

        //Connect UI Signals
        _saveConfigBtn.Pressed += OnSaveConfigBtnPressed;
        _loadConfigBtn.Pressed += OnLoadConfigBtnPressed;
        _spriteGenFolderPathLineEdit.TextChanged += (newDir) => GlobalUtil.OnFolderSelected(newDir, _spriteGenFolderPathLineEdit);
        _selectFolderPathBtn.Pressed += OnSelectFolderPathPressed;
        _openFolderPathBtn.Pressed += OnOpenFolderPathPressed;
        _openSettingPanelBtn.Pressed += () => _settingsMainPanel.Visible = !_settingsMainPanel.Visible;
        _settingsMainPanel.Visible = false;
        _spriteGenFolderPathLineEdit.Text = GlobalUtil.SaveFolderPath;

        GlobalUtil.SaveFolderPath = ProjectSettings.GlobalizePath(Const.SAVE_GAME_PATH);
        _spriteGenFolderPathLineEdit.Text = GlobalUtil.SaveFolderPath;

    }

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

        //fileDialog.DirSelected += (newDir) => GlobalUtil.OnFolderSelected(newDir, _spriteGenFolderPathLineEdit);

        //fileDialog.FileSelected += async (path) => await SaveGameManager.Instance.LoadGameData(path);

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        fileDialog.PopupCentered();

        await ToSignal(fileDialog, FileDialog.SignalName.FileSelected);

        string file = fileDialog.CurrentFile;
        string fullLoadFilePath = fileDialog.CurrentDir + "/" + file;

        await SaveGameManager.Instance.LoadGameData(fullLoadFilePath);

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

        if (!GlobalUtil.HasDirectory(globalizedSavePath, this))
        {
            GD.Print("Directory does NOT exist: " + globalizedSavePath);
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
        if (GlobalUtil.HasDirectory(directory, this))
        {
            OS.ShellOpen(directory);
        }
        else
        {
            GD.PrintErr("Directory does not exist: " + directory);

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
