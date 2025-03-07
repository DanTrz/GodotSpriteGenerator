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
    [Export] private int SpriteResolution { get; set; }

    [Export] private int FrameSkipStep { get; set; }
    [Export] private bool ClearFolderBeforeGeneration { get; set; }
    [Export] private bool UsePixelEffect { get; set; }

    [Export] private int PlaybackSpeed { get; set; }

    //MODEL POSITION MANAGER VARIABLES AND CONFIG
    [Export] private float CameraDistance { get; set; }
    [Export] private float CameraRotation { get; set; }

    [Export] private float ModelPositionXAxis { get; set; }
    [Export] private float ModelPositionYAxis { get; set; }
    [Export] private float ModelPositionZAxis { get; set; }

    [Export] private float ModelRotationXAxis { get; set; }
    [Export] private float ModelRotationYAxis { get; set; }
    [Export] private float ModelRotationZAxis { get; set; }

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
