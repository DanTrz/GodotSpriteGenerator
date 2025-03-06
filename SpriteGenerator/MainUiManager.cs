using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;


public partial class MainUiManager : Control
{

    //public static MainUiManager Instance { get; private set; }
    public static MainUiManager Instance { get; private set; }

    // [Export] public SpriteGenerationUiManager SpriteGenUIManager;
    // [Export] public SpriteSheetGenUiManager SpriteSheetGenUIManager;
    // //Save Config
    [Export] private Button _saveConfigBtn;
    [Export] private Button _loadConfigBtn;

    //Main Settings and Folder Path Variables
    [OnReady("%SelectFolderPathBtn")] private Button _selectFolderPathBtn;
    [OnReady("%OpenFolderPathBtn")] private Button _openFolderPathBtn;
    [OnReady("%SpriteGenFolderPathLineEdit")] LineEdit _spriteGenFolderPathLineEdit;
    [OnReady("%SettingsMainPanel")] MarginContainer _settingsMainPanel;
    [OnReady("%OpenSettingPanelBtn")] Button _openSettingPanelBtn;

    private async Task OnLoadConfigPressed()
    {
        GD.Print("LoadButton pressed");
        await SaveGameManager.Instance.LoadGameData();
    }

    private async Task OnSaveConfigPressed()
    {
        GD.Print("SaveButton pressed");
        await SaveGameManager.Instance.SaveGameData();
    }


    public override void _Ready()
    {
        Instance = this;


        GD.PrintT("Tried Reading Node:  " + this.Name);

        _spriteGenFolderPathLineEdit.TextChanged += (newDir) => GlobalUtil.OnFolderSelected(newDir, _spriteGenFolderPathLineEdit);

        _selectFolderPathBtn.Pressed += OnSelectFolderPathPressed;
        _openFolderPathBtn.Pressed += OnOpenFolderPathPressed;
        _openSettingPanelBtn.Pressed += () => _settingsMainPanel.Visible = !_settingsMainPanel.Visible;

        _settingsMainPanel.Visible = false;
        _spriteGenFolderPathLineEdit.Text = GlobalUtil.SaveFolderPath;
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
