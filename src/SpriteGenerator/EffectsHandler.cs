using System;
using System.Collections.Generic;
using Godot;

public static class EffectsHandler
{

    private static BaseMaterial3D.ShadingModeEnum _shadingMode; //Unsahded / PerPixel
    private static BaseMaterial3D.TextureFilterEnum _textureFilter; // Nearest
    private static BaseMaterial3D.DiffuseModeEnum _diffuseMode; //Toon
    private static BaseMaterial3D.SpecularModeEnum _specularMode; //Toon
    private static BaseMaterial3D.TransparencyEnum _transparency; //Disabled
    private static bool _receiveShadows; //Disabled
    private static bool __emissionEnabled;
    private static float _roughnessValue;
    private static float _rimValue;
    private static void PrepareUnshadedVariables()
    {
        //GD.PrintT("Prepare UnShaded Effect ShadedVariables");
        _shadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
        _textureFilter = BaseMaterial3D.TextureFilterEnum.Nearest;
        _diffuseMode = BaseMaterial3D.DiffuseModeEnum.Toon;
        _specularMode = BaseMaterial3D.SpecularModeEnum.Toon;
        _transparency = BaseMaterial3D.TransparencyEnum.Disabled;
        _receiveShadows = false;
        __emissionEnabled = false;
        _roughnessValue = 0.0f; //BaseMaterial3D.TextureParam.Roughness; ////0 to 1 float
        _rimValue = 0.0f; //BaseMaterial3D.TextureParam.Rim;////0 to 1 float
    }

    private static void PrepareToonShadedVariables()
    {
        //GD.PrintT("Prepare Toon Effect ShadedVariables");
        _shadingMode = BaseMaterial3D.ShadingModeEnum.PerPixel;
        _textureFilter = BaseMaterial3D.TextureFilterEnum.Nearest;
        _diffuseMode = BaseMaterial3D.DiffuseModeEnum.Toon;
        _specularMode = BaseMaterial3D.SpecularModeEnum.Toon;
        _transparency = BaseMaterial3D.TransparencyEnum.Disabled;
        _receiveShadows = true;
        __emissionEnabled = false;
        _roughnessValue = 0.8f; //BaseMaterial3D.TextureParam.Roughness; ////0 to 1 float
        _rimValue = 0.2f; //BaseMaterial3D.TextureParam.Rim;////0 to 1 float
    }

    public static void SetEffect(this Node3D node, Const.EffectShadingType effectType)
    {
        List<MeshInstance3D> allMesheInstanced3D = GlobalUtil.GetAllNodesByType<MeshInstance3D>(node);

        foreach (var meshInstance3D in allMesheInstanced3D)
        {
            switch (effectType)
            {
                case Const.EffectShadingType.UNSHADED:
                    meshInstance3D.CastShadow = MeshInstance3D.ShadowCastingSetting.Off;
                    PrepareUnshadedVariables();
                    break;
                case Const.EffectShadingType.TOON:
                    meshInstance3D.CastShadow = MeshInstance3D.ShadowCastingSetting.On;
                    PrepareToonShadedVariables();
                    break;
            }

            ApplyEffectsToMesh(meshInstance3D.Mesh);
        }
    }

    private static void ApplyEffectsToMesh(Mesh mesh)
    {
        SurfaceTool surfaceTool = new SurfaceTool();
        ArrayMesh arrayMesh = (ArrayMesh)mesh;
        int surfaceCount = arrayMesh.GetSurfaceCount();

        for (int surfaceIndex = 0; surfaceIndex < surfaceCount; surfaceIndex++)
        {
            if (arrayMesh.SurfaceGetMaterial(surfaceIndex) is StandardMaterial3D myMaterial3D)
            {
                myMaterial3D.ShadingMode = _shadingMode;
                myMaterial3D.TextureFilter = _textureFilter;
                myMaterial3D.DiffuseMode = _diffuseMode;
                myMaterial3D.SpecularMode = _specularMode;
                myMaterial3D.Transparency = _transparency;
                myMaterial3D.DisableReceiveShadows = _receiveShadows;
                myMaterial3D.EmissionEnabled = __emissionEnabled;
                myMaterial3D.Roughness = _roughnessValue;
                myMaterial3D.Rim = _rimValue;

                arrayMesh.SurfaceSetMaterial(surfaceIndex, myMaterial3D);
                //GD.PrintT("Mesh Effects Applied to: " + mesh.ResourceName);
            }
        }

        GD.PrintT("Mesh Effects Applied to " + surfaceCount + " Surfaces on: " + mesh.ResourceName);

    }
}
