using System.Collections.Generic;

public static class Const
{
    public const string MOUSE_RIGHT_CLICK = "MouseRightClick";

    public static Dictionary<int, float> ImgRenderTimeInterval = new()
    {
        { 1, 0.031f },
        { 2, 0.015f },
        { 3, 0.007f },
        { 4, 0.003f },
    };

    public const string HEAD_ORIG_MESH_PATH = "res://Models/MeshRepository/Head/HeadMesh_Original.res";
    public const string HEAD_YELLOW_MESH_PATH = "res://Models/MeshRepository/Head/HeadMesh_Yellow.res";

    public const string TORSO_ORIG_MESH_PATH = "res://Models/GirlManualRig/MeshOriginal/NoHairGirlSeparateParts01_TorsoBodyMesh.res";
    public const string TORSO_YELLOW_MESH_PATH = "res://Models/GirlManualRig/MeshYellow/YellowNoHairGirlSeparateParts01_TorsoBodyMesh.res";


    public const string HEAD_MESHES_FOLDER_PATH = "res://Models/MeshRepository/Head/";
    public const string TORSO_MESHES_FOLDER_PATH = "res://Models/MeshRepository/TorsoBody/";
    public const string HAIR_MESHES_FOLDER_PATH = "res://Models/MeshRepository/Hair/";


    // 0.031f 
}
