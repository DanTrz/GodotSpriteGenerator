using Godot;

[GlobalClass]
[Tool]
public partial class ArrayMeshDataObject : Resource
{

    [Export] public string ItemName;
    [Export] public ArrayMesh MeshItem;
    [Export] public Const.BodyPartType BodyPartType = Const.BodyPartType.HEAD;
    [Export] public Texture2D Icon;
    [Export] public int ItemOrder = 1;

}