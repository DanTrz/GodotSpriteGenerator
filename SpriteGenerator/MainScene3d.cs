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

		MainAnimationPlayer = MainCharacterObj.GetNodeOrNull<AnimationPlayer>("%AnimationPlayer");

		if (MainAnimationPlayer == null)
		{
			string animationPlayerPath = ResolveAnimationPlayerPath(MainCharacterObj);
			MainAnimationPlayer = MainCharacterObj.GetNodeOrNull<AnimationPlayer>(animationPlayerPath);

			if (MainAnimationPlayer == null)
			{
				GD.PrintErr("AnimationPlayer missing in selected Main character in MainScene3d");
			}
		}
	}

	public string ResolveAnimationPlayerPath(Node _owner)
	{
		foreach (var node in _owner.GetChildren())
		{
			if (node is AnimationPlayer animPlayer)
			{
				return animPlayer.GetPath().ToString();
			}
		}

		return "";
	}


}
