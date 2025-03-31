using Godot;

[GlobalClass]
public partial class ArrayMeshDataObject : Resource
{

    [Export] public bool Active = false;
    [Export] public string ItemName;
    [Export] public ArrayMesh MeshItem;
    [Export] public Const.BodyPartType BodyPartType = Const.BodyPartType.HEAD;
    [Export] public Const.MeshType MeshType = Const.MeshType.LOW_POLY;
    [Export] public Texture2D Icon;
    [Export] public int ItemOrder = 1;
    [Export] public bool CanChangeColor = false;

}