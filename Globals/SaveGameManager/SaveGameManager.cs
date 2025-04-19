using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;

public partial class SaveGameManager : Node
{
    public static SaveGameManager Instance { get; private set; }

    public static SpriteGenerator SpriteGenParentNode { get; private set; }
    public static ImageEditorMainPanel ImgEditorParentNode { get; private set; }

    //public Godot.Collections.Array nodeSaveData { get; private set; }
    public Godot.Collections.Dictionary<string, Dictionary> nodeSaveData { get; private set; }

    public override void _Ready() { Instance = this; }

    public async Task SaveGameData(string fullSaveFilePath)
    {
        Log.Debug("SaveGamedata started");
        if (!UpdatedParentNodesSucceeded()) return;

        SaveGameData newSaveGameData = new();

        //TODO: Add option to Save different with choosen files names etc...
        newSaveGameData.SaveFileName = fullSaveFilePath;
        newSaveGameData.GetSaveGameDataFromNodes(SpriteGenParentNode, ImgEditorParentNode);

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        nodeSaveData = new();

        GetTree().CallGroup("save_data", SpriteGenerator.MethodName.OnSaveData, newSaveGameData);

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        //ResourceSaver.Save(newSaveGameData, Const.SAVE_GAME_PATH + newSaveGameData.SaveFileName);
        ResourceSaver.Save(newSaveGameData, fullSaveFilePath);

    }

    public async Task LoadGameDataFromPath(string fullLoadFilePath)
    {
        if (!UpdatedParentNodesSucceeded()) return;

        Log.Debug("LoadGamedata started");
        SaveGameData newLoadSaveGameData = new();

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        //TODO: Change so we can load different files
        newLoadSaveGameData = ResourceLoader.Load<SaveGameData>(fullLoadFilePath);

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        //Push the data from the SaveGame file to the game
        newLoadSaveGameData.LoadSaveGameDataToNodes(newLoadSaveGameData, SpriteGenParentNode, ImgEditorParentNode);

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        GetTree().CallGroup("save_data", SpriteGenerator.MethodName.OnLoadData, newLoadSaveGameData);


        // Log.Debug("LoadGamedata Loaded: " + newLoadSaveGameData.SaveFileName);
    }

    private bool UpdatedParentNodesSucceeded()
    {
        var mainUI = GetTree().Root.GetNodeOrNull("MainInterfaceUI/MainUI");

        //SpriteGenParentNode = mainUI.GetNodeOrNull<SpriteGenerator>("%SpriteGenerator");
        //ImgEditorParentNode = mainUI.GetNodeOrNull<ImageEditorMainPanel>("%ImageEditor");
        SpriteGenParentNode = GlobalUtil.GetAllChildNodesByType<SpriteGenerator>(mainUI).ToList().FirstOrDefault();
        ImgEditorParentNode = GlobalUtil.GetAllChildNodesByType<ImageEditorMainPanel>(mainUI).ToList().FirstOrDefault();

        if (ImgEditorParentNode == null || SpriteGenParentNode == null)
        {
            Log.Error("SpriteGenParentNode or ImgEditorParentNode is null");
            return false;
        }

        return true;

    }


}
