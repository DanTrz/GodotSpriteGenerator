using System;
using System.Collections.Generic;
using Godot;

public partial class GlobalEvents : Node
{
    public static GlobalEvents Instance { get; private set; }
    public override void _Ready() { Instance = this; }

    public Action<Godot.Collections.Array<Color>> OnPaletteChanged { get; set; }

    public Func<Godot.Collections.Array<Color>> OnGetTextureFromImgCompleted { get; set; }

    public Action<string> OnEffectsChangesStarted { get; set; }

    public Action<string, Godot.Collections.Array<Color>> OnEffectsChangesEnded { get; set; }
}
