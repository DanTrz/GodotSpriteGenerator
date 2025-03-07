using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;

public partial class SaveGameManager : Node
{
    public static SaveGameManager Instance { get; private set; }

    //public Godot.Collections.Array nodeSaveData { get; private set; }
    public Godot.Collections.Dictionary<string, Dictionary> nodeSaveData { get; private set; }

    public override void _Ready() { Instance = this; }

    public async Task SaveGameData()
    {

        GD.Print("SaveGamedata started");
        SaveGameData newSaveGameData = new();

        //TODO: Change so we can Save different files names etc...
        newSaveGameData.SaveFileName = "savegamedata.tres";

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        //List<SaveGameData> newList = new();
        // get_tree().call_group("persistent_data", "on_save_game", persistent_data_objects)
        nodeSaveData = new();
        //nodeSaveData2 = new();

        GetTree().CallGroup("save_data", SpriteGenerator.MethodName.OnSaveData, newSaveGameData);

        // //GetTree().CallGroup("save_data", SpriteGenerator.MethodName.OnSaveData2);

        // GD.PrintT(nodeSaveData.Count.ToString());
        // var spriteRes = nodeSaveData[nameof(SpriteGenerator)].GetValueOrDefault(nameof(SpriteGenerator._spriteResolution)).ToString();
        // //
        // var CamDistance = nodeSaveData[nameof(ModelPositionManager)].GetValueOrDefault("CameraDistance").ToString();
        // var CamDistance2 = nodeSaveData[nameof(ModelPositionManager)].GetValueOrDefault(nameof(ModelPositionManager.CamDistancelLineTextEdit)).ToString();
        // var _showGridCheckButton = nodeSaveData[nameof(SpriteGenerator)].GetValueOrDefault(nameof(SpriteGenerator._showGridCheckButton)).ToString();


        // var _pixelEffectCheckBtn = nodeSaveData[nameof(SpriteGenerator)].GetValueOrDefault(nameof(SpriteGenerator._pixelEffectCheckBtn)).ToString();

        // newSaveGameData.ShowPixelEffect = (bool)nodeSaveData[nameof(SpriteGenerator)].GetValueOrDefault(nameof(SpriteGenerator._pixelEffectCheckBtn));

        // GD.PrintT(spriteRes, CamDistance, CamDistance2, _showGridCheckButton, _pixelEffectCheckBtn);

        //Get the data from the Game (Nodes, etc) and sets it to the SaveGame file
        newSaveGameData.GetSaveGameDataFromNodes();

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        ResourceSaver.Save(newSaveGameData, Const.SAVE_GAME_PATH + newSaveGameData.SaveFileName);

    }

    public async Task LoadGameData()
    {
        GD.Print("LoadGamedata started");
        SaveGameData newLoadSaveGameData = new();

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        //TODO: Change so we can load different files
        newLoadSaveGameData = ResourceLoader.Load<SaveGameData>(Const.SAVE_GAME_PATH + "savegamedata.tres");

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        GetTree().CallGroup("save_data", SpriteGenerator.MethodName.OnLoadData, newLoadSaveGameData);

        //Push the data from the SaveGame file to the game
        newLoadSaveGameData.SetSaveGameDataToNodes(newLoadSaveGameData);
    }

    //TODO: Consider implementing 

    //1. CALL GROUP TO GET THEIR DATA TO SAVE (ON SAVE GAME)
    //1.1 This also means I need to start saving the actual Nodes (Objets) as well not only properties (Much more complex)
    // # This function runs the on_save_game for all objects in the group "persistent_data" and save to persistent_data_objects
    //     get_tree().call_group("persistent_data", "on_save_game", persistent_data_objects)


    //2. CALL GROUP TO CLEAR / RESET WHATEVER DATSA IS NEEDED BEFORE LOADING (Optional I think)
    // # Clear all objects we want to reload from persistent data
    //     get_tree().call_group("persistent_data", "on_before_load_game")

    //3. LOOP THROUGHT THE SAVE GAME DATA NODES AND CALL THE ON LOAD GAME METHOD
    // if restore_node.has_method("on_load_game"):
    // restore_node.is_used_open_or_dead = data.is_used_open_or_dead
    // restore_node.on_load_game(data)


    //CONSIDER USING C# COOL STUFF LIKE mIXINS OR INTERFACES TO DO THIS FOR EVERY NODE i NEED 


}
