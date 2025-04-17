using System;
using Godot;


[Tool]
public partial class HdSubViewport : SubViewport
{
    [Export] public TextureRect HDTextureRect;

    public override void _Ready()
    {
        //HDTextureRect.Texture.Changed += OnHDTextureRectChanged;
    }

    private void OnHDTextureRectChanged()
    {
        Vector2I textureRectImageSize = HDTextureRect.Texture.GetImage().GetSize();
        if (this.Size != textureRectImageSize)
        {
            //Log.Debug("Texture Changed on HDTextureRect");
            this.Size = textureRectImageSize;
            //Log.Debug("HDSubViewPort Sized Changed to: " + this.Size);

        }

    }

    public override void _Process(double delta)
    {
        OnHDTextureRectChanged();
    }



}
