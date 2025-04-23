using System;
using System.Collections.Generic;
using Godot;

public partial class GlobalEvents : Node
{
    //TODO: REFACTOR CODE: Consider change the Actions to event Action and handling the null checks and invoke within this Class.
    public static GlobalEvents Instance { get; private set; }
    public override void _Ready() { Instance = this; }

    public Action<Godot.Collections.Array<Color>> OnPaletteChanged { get; set; }

    public Action<string, Const.BodyPartType, Color> OnMeshItemColorChanged { get; set; }

    public Action<string> OnEffectsChangesStarted { get; set; }

    public Action<string, Godot.Collections.Array<Color>> OnEffectsChangesEnded { get; set; }

    public Action<int> OnForcedPaletteSize { get; set; }

    public Action<Image> OnSpriteSheetCreated { get; set; }

    public Action<int, Vector3> OnModelTransformChanged { get; set; }

    public Action OnCamResetChanges { get; set; }

    public Action<float> OnSpriteSizeChanged { get; set; }


}
