using Godot;

public partial class NoHairGirlParts : Node3D
{

    [OnReady("%Head")] public MeshInstance3D HeadMesh;
    [OnReady("GirlNoHairArmature/GeneralSkeleton/TorsoBody")] public MeshInstance3D TorsoMesh;

    const string HeadOriginnalMeshPath = "res://Models/GirlManualRig/MeshOriginal/NoHairGirlSeparateParts01_HeadMesh.res";
    const string HeadYellowMeshPath = "res://Models/GirlManualRig/MeshYellow/YellowNoHairGirlSeparateParts01_HeadMesh.res";

    const string TorsoOriginnalMeshPath = "res://Models/GirlManualRig/MeshOriginal/NoHairGirlSeparateParts01_TorsoBodyMesh.res";
    const string TorsoYellowMeshPath = "res://Models/GirlManualRig/MeshYellow/YellowNoHairGirlSeparateParts01_TorsoBodyMesh.res";


    public override void _Ready()
    {
        if (HeadMesh == null || TorsoMesh == null)
        {
            HeadMesh = GetNodeOrNull<MeshInstance3D>("%Head");
            TorsoMesh = GetNodeOrNull<MeshInstance3D>("GirlNoHairArmature/GeneralSkeleton/TorsoBody");
        }

        HeadMesh.Mesh = GD.Load<Mesh>(HeadYellowMeshPath);
        TorsoMesh.Mesh = GD.Load<Mesh>(TorsoYellowMeshPath);
    }

}
