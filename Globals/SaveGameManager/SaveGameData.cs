using System;
using Godot;

[GlobalClass]
public partial class SaveGameData : Resource
{
    //SPRITE GENERATOR VARIABLES AND CONFIG
    [Export] public string SaveFileName { get; set; }
    [Export] public bool ShowGrid { get; set; }
    [Export] public bool ShowPixelEffect { get; set; }

    [Export] public int PixelEffectLevel { get; set; }
    [Export] public int SpriteResolution { get; set; }
    [Export] public int SpriteResolutionOptBtn { get; set; }

    [Export] public int FrameSkipStep { get; set; }
    [Export] public bool ClearFolderBeforeGeneration { get; set; }
    [Export] public bool UsePixelEffect { get; set; }

    [Export] public float PlaybackSpeed { get; set; }

    //MODEL POSITION MANAGER VARIABLES AND CONFIG
    [Export] public float CameraDistance { get; set; }
    [Export] public float CameraRotation { get; set; }

    [Export] public float ModelPositionXAxis { get; set; }
    [Export] public float ModelPositionYAxis { get; set; }
    [Export] public float ModelPositionZAxis { get; set; }

    [Export] public float ModelRotationXAxis { get; set; }
    [Export] public float ModelRotationYAxis { get; set; }
    [Export] public float ModelRotationZAxis { get; set; }

    //SPRITE SHEET GEN VARIABLES AND CONFIG
    // [Export] private int SpriteResolution { get; set; }
    // [Export] private int SpriteResolution { get; set; }
    // [Export] private int SpriteResolution { get; set; }
    // [Export] private int SpriteResolution { get; set; }



    //Used by SAVEGAMEMANAGER to Save Data for a new SaveGame file
    public void GetSaveGameDataFromNodes()
    {


    }

    //Used by SAVEGAMEMANAGER to LOAD Data from a SaveGame file to the Game, Nodes, Scenes. 
    public void SetSaveGameDataToNodes(SaveGameData saveGameDataToLoad)
    {


    }
}
