using Godot;

[Tool]
public partial class BodyPartMeshInstance3D : MeshInstance3D
{
    [Export] public Const.BodyPartType BodyPartType = Const.BodyPartType.HEAD;
    [Export] public Const.MeshType MeshType = Const.MeshType.LOW_POLY;

    private Mesh myMesh;

    public override void _Ready()
    {
        myMesh = this.Mesh;

        if (MeshType == Const.MeshType.LOW_POLY)
        {
            PrepareLowPolyMesh();
        }
        else
        {
            PrepareHighPolyMesh();
        }
    }

    /*************  ✨ Codeium Command 🌟  *************/
    private void PrepareLowPolyMesh()
    {

        //1. Check All Surface and get Materials that his mesh has
        //2. for each Material we will update the material properties like: ShadingMode etc.

        this.CastShadow = ShadowCastingSetting.Off;

        SurfaceTool surfaceTool = new SurfaceTool();
        ArrayMesh arrayMesh = (ArrayMesh)myMesh;
        int surfaceCount = arrayMesh.GetSurfaceCount();

        for (int surfaceIndex = 0; surfaceIndex < surfaceCount; surfaceIndex++)
        {
            if (arrayMesh.SurfaceGetMaterial(surfaceIndex) is StandardMaterial3D myMaterial3D)
            {
                // myMaterial3D.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
                // myMaterial3D.TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest;
                // myMaterial3D.EmissionEnabled = false;


                // arrayMesh.SurfaceSetMaterial(surfaceIndex, myMaterial3D);
            }
        }
    }

    private void PrepareHighPolyMesh()
    {

    }

}
