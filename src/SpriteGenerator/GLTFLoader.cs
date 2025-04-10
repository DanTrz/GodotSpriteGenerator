using System;
using System.Threading.Tasks;
using Godot;


public partial class GLTFLoader : Node
{


    public static async Task LoadExternalGLTF(string filePath, Node parentNode)
    {
        var gltfDocumentLoad = new GltfDocument();
        var gltfStateLoad = new GltfState();
        gltfStateLoad.BasePath = Const.RES_TEMPSAVE_FOLDER_PATH;

        var error = gltfDocumentLoad.AppendFromFile(filePath, gltfStateLoad);
        if (error == Error.Ok)
        {
            var gltfSceneRootNode = gltfDocumentLoad.GenerateScene(gltfStateLoad);

            await parentNode.ToSignal(parentNode.GetTree(), SceneTree.SignalName.ProcessFrame);

            parentNode.AddChild(gltfSceneRootNode);

            await parentNode.ToSignal(parentNode.GetTree(), SceneTree.SignalName.ProcessFrame);

            GD.PrintT($"External Model loaded from: {filePath}");

        }
        else
        {
            GD.PrintErr($"Couldn't load glTF scene (error code: {error}).");
        }


        gltfDocumentLoad.Dispose();
        gltfStateLoad.Dispose();


    }
}
