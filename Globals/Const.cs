using Godot;

public static class Const
{
    public const string MOUSE_RIGHT_CLICK = "MouseRightClick";

    public static readonly string RES_ROOT_FOLDER_PATH = ProjectSettings.GlobalizePath("res://");
    public static readonly string RES_TEMPSAVE_FOLDER_PATH = ProjectSettings.GlobalizePath("res://SpriteSheets");
    public static readonly string USER_ROOT_FOLDER_PATH = ProjectSettings.GlobalizePath("user://");

    public const string HEAD_ORIG_MESH_PATH = "res://Models/MeshRepository/Head/HeadMesh_Original.res";
    public const string HEAD_YELLOW_MESH_PATH = "res://Models/MeshRepository/Head/HeadMesh_Yellow.res";

    public const string TORSO_ORIG_MESH_PATH = "res://Models/GirlManualRig/MeshOriginal/NoHairGirlSeparateParts01_TorsoBodyMesh.res";
    public const string TORSO_YELLOW_MESH_PATH = "res://Models/GirlManualRig/MeshYellow/YellowNoHairGirlSeparateParts01_TorsoBodyMesh.res";


    public const string HEAD_MESHES_FOLDER_PATH = "res://Models/MeshRepository/Head/";
    public const string TORSO_MESHES_FOLDER_PATH = "res://Models/MeshRepository/TorsoBody/";
    public const string HAIR_MESHES_FOLDER_PATH = "res://Models/MeshRepository/Hair/";
    public const string LEGS_MESHES_FOLDER_PATH = "res://Models/MeshRepository/Leg/";
    public const string FEET_MESHES_FOLDER_PATH = "res://Models/MeshRepository/Feet/";
    public const string LEFT_ARM_MESHES_FOLDER_PATH = "res://Models/MeshRepository/LeftArm/";
    public const string RIGHT_ARM_MESHES_FOLDER_PATH = "res://Models/MeshRepository/RightArm/";
    public const string MESH_REPO_FOLDER_PATH = "res://Models/MeshRepository/";
    //


    //Custom Mesh Data
    public enum BodyPartType { HEAD, TORSO, HAIR, LEGS, FEET, LEFT_ARM, RIGHT_ARM };

    // 0.031f 
}
