using Godot;
using System;

public partial class MainScene3d : Node3D
{
	[Export] public Node3D MainModelNode;
	[Export] public Node3D MainCharacterObj;
	[Export] public Camera3D MainCamera;
	public AnimationPlayer MainAnimationPlayer;

	public override void _Ready()
	{
		if (MainModelNode == null || MainCamera == null)
		{
			GD.PrintErr("Null values in MainScene3d - Check MainModel and MainCamera Exported properties");
		}

		MainAnimationPlayer = MainCharacterObj.GetNode<AnimationPlayer>("%AnimationPlayer");

		if (MainAnimationPlayer == null)
		{
			GD.PrintErr("AnimationPlayer missing in selected Main character in MainScene3d");
		}
	}


}
