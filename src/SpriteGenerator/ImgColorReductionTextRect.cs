using System;
using System.Threading.Tasks;
using Godot;

[Tool]
public partial class ImgColorReductionTextRect : TextureRect
{
    [ExportToolButton("UpdateShader")] public Callable ClickMeButton => Callable.From(UpdateShaderParameters);
    [Export] public bool EnableColorReduction = true;
    [Export] public int NumColors = 256;
    [Export] public bool UseExternalPalette = true;
    [Export] public Godot.Collections.Array<Color> ShaderPalette = new();

    public async Task<bool> UpdateShaderParameters()
    {
        if (this.Material is not ShaderMaterial shaderMaterial)
        {
            GD.PrintErr("Material is not a ShaderMaterial.");
            return false;
        }

        ImageEditor _localImgEditor = new();
        _localImgEditor.UpdatedKMeansClustering(this.Texture.GetImage(), NumColors);
        ShaderPalette = _localImgEditor.ColorListToGodotArray(_localImgEditor.ImgkMeansClusterList);

        shaderMaterial.SetShaderParameter("enable_color_reduction", EnableColorReduction);
        shaderMaterial.SetShaderParameter("num_colors", NumColors);
        shaderMaterial.SetShaderParameter("palette", ShaderPalette);

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        return true;


    }

}
