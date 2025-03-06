using Godot;

public static class Const
{
    public const string MOUSE_RIGHT_CLICK = "MouseRightClick";

    public static readonly string RES_ROOT_FOLDER_PATH = ProjectSettings.GlobalizePath("res://");
    public static readonly string RES_TEMPSAVE_FOLDER_PATH = ProjectSettings.GlobalizePath("res://SpriteSheets");
    public static readonly string USER_ROOT_FOLDER_PATH = ProjectSettings.GlobalizePath("user://");

    // Mesh Data
    public enum BodyPartType { HEAD, TORSO, HAIR, LEGS, FEET, LEFT_ARM, RIGHT_ARM };
    public const string HAIR_SCENES_FOLDER_PATH = "res://Models/MeshRepository/Hair/";
    public const string MESH_REPO_FOLDER_PATH = "res://Models/MeshRepository/";

    //Transform Data
    public enum TransformType { CAMERA_ZOOM, SCALE, POSITION, ROTATION };
    public enum TransformAxis { X, Y, Z };

    //SAVE Data
    public const string SAVE_GAME_PATH = "user://";



}
