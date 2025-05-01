using Godot;

public static class Const
{
    public static class Models
    {
        public static readonly string BARBARIAN = "uid://buurhphmqc16s";
        public static readonly string GODOT_PLUSH = "uid://c1c2k76x0pauf";
        public static readonly string LOW_POLY = "uid://bvvxjc2oqng0j";

        public static readonly string MEDIUM_POLY = "uid://cupe4j2fy060e";

    }

    public const string MOUSE_RIGHT_CLICK = "MouseRightClick";

    public static readonly string RES_ROOT_FOLDER_PATH = ProjectSettings.GlobalizePath("res://");
    public static readonly string RES_TEMPSAVE_FOLDER_PATH = ProjectSettings.GlobalizePath("res://Assets/Resources/TempResources/");
    public static readonly string USER_ROOT_FOLDER_PATH = ProjectSettings.GlobalizePath("user://");

    // Mesh Data

    public enum BodyPartType { HEAD, TORSO, HAIR, LEGS, FEET, LEFT_ARM, RIGHT_ARM, WEAPON };
    public enum MeshType { LOW_POLY, HIGH_POLY };

    public enum EffectShadingType { STANDARD, UNSHADED, TOON };
    public const string HAIR_SCENES_FOLDER_PATH = "res://Models/MeshRepository/Hair/";
    public const string WEAPON_SCENES_FOLDER_PATH = "res://Models/MeshRepository/Weapons/";
    public const string MESH_REPO_FOLDER_PATH = "res://Models/MeshRepository/";
    public const string SPRITESHEET_TEMP_FOLDER_PATH = "res://Assets/SpriteSheetTemp/";
    public const string PRESET_SAVEDATA_FOLDER_PATH = "res://Assets/Resources/PresetDataSaves/";
    public const string PRESET_NONE = "None.tres";
    public const string PRESET_NONE_FILE_PATH = "res://Assets/Resources/PresetDataSaves/Default.tres";

    //Transform Data
    public enum TransformType { CAMERA_ZOOM, SCALE, POSITION, ROTATION };
    public enum TransformAxis { X, Y, Z };

    //SAVE Data
    public const string SAVE_GAME_PATH = "user://";

}
