using Godot;
using System;

public partial class MainScene3d : Node3D
{
	[Export] public Node3D MainModel;
	[Export] public Camera3D MainCamera;

	public override void _Ready()
	{
		if (MainModel == null || MainCamera == null)
		{
			GD.PrintErr("Null values in MainScene3d - Check MainModel and MainCamera Exported properties");
		}
	}


}
