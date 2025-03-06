using System;
using Godot;

[GlobalClass]
public partial class SaveGameData : Resource
{
    [Export] public string SaveFileName { get; set; }
    [Export] public bool ShowGrid { get; set; }
    [Export] public bool PixelEffect { get; set; }


    //Used by SAVEGAMEMANAGER to Save Data for a new SaveGame file
    public void GetSaveGameDataFromNodes()
    {


    }

    //Used by SAVEGAMEMANAGER to LOAD Data from a SaveGame file to the Game, Nodes, Scenes. 
    public void SetSaveGameDataToNodes(SaveGameData saveGameDataToLoad)
    {


    }
}
