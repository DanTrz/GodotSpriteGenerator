using Godot;
using System;

public partial class RootScene : Node
{
	[Export(PropertyHint.File, "*.tscn")]
	public string MainScene;

	public override void _Ready()
	{
		Callable.From(() => GetTree().ChangeSceneToFile(MainScene)).CallDeferred();

		// CallDeferred(nameof(LoadMainScene));


	}

	// private void LoadMainScene()
	// {
	// 	GetTree().ChangeSceneToFile(MainScene);
	// }


}