using System.Collections.Generic;
using System.Linq;
using Godot;
public partial class MeshReplacer : Node
{

    private static int _iconSize = 32;
    public static List<ArrayMeshDataObject> arrayMeshDataObjects;

    public override void _Ready()
    {
        LoadAllMeshDataResouces();
        GlobalEvents.Instance.OnMeshItemColorChanged += UpdateMeshItemColor;
    }

    private static void LoadAllMeshDataResouces()
    {
        //GetAllMeshItem data that are ArrayMeshDataObject type and load to a local list
        arrayMeshDataObjects = GlobalUtil.GetResourcesByType<ArrayMeshDataObject>(Const.MESH_REPO_FOLDER_PATH);
        arrayMeshDataObjects.Sort((a, b) => a.ItemOrder.CompareTo(b.ItemOrder));
    }

    public static void UpdateMeshItemColor(string itemSelectedName, Color newColor)
    {

        if (GetMeshItemByName(itemSelectedName, arrayMeshDataObjects).SurfaceGetMaterial(0) is StandardMaterial3D material)
        {
            material.AlbedoColor = newColor;

        }
    }


    public static void UpdateUIOptionMesheItemList(MeshReplacerOptButton itemMeshOptBtn, Const.BodyPartType bodyPart)
    {
        if (arrayMeshDataObjects == null)
        {
            LoadAllMeshDataResouces();
        }

        var bodyPartMeshes = arrayMeshDataObjects.Where(
            mesh => mesh.BodyPartType == itemMeshOptBtn.BodyPartType
            && mesh.Active == true).ToList();

        int itemId = 1;
        foreach (ArrayMeshDataObject item in bodyPartMeshes)
        {
            itemMeshOptBtn.AddItem(item.ItemName, itemId);

            int itemIndex = itemMeshOptBtn.GetItemIndex(itemId);

            Texture2D icon = item.Icon;
            if (icon != null)
            {
                // Resize the icon to a smaller size
                Image iconImage = icon.GetImage();
                iconImage.Resize(_iconSize, _iconSize); // Set the desired size here
                Texture2D resizedIcon = ImageTexture.CreateFromImage(iconImage);

                itemMeshOptBtn.SetItemIcon(itemIndex, resizedIcon);
            }

            if (item.CanChangeColor)
            {
                itemMeshOptBtn.EnableColorPicker(true);
            }

            itemId++;
        }
    }

    public static void UpdateMeshFromResourceItem(MeshInstance3D meshTargetToReplace, string resourceItemName)
    {

        if (meshTargetToReplace == null || meshTargetToReplace.Mesh == null)
        {
            GD.PrintErr(meshTargetToReplace.Name + " has no mesh or is null");
        }

        meshTargetToReplace.Mesh = GetMeshItemByName(resourceItemName, arrayMeshDataObjects);

    }

    // public static void UpdateTargetMesh(MeshInstance3D meshTargetToReplace, Mesh newMeshItem)
    // {

    //     if (meshTargetToReplace == null || meshTargetToReplace.Mesh == null)
    //     {
    //         GD.PrintErr(meshTargetToReplace.Name + " has no mesh or is null");
    //     }

    //     meshTargetToReplace.Mesh = newMeshItem;

    // }

    public static Mesh GetMeshItemByName(string itemResName, List<ArrayMeshDataObject> resourceMeshList)
    {
        var mesh = resourceMeshList.Where(MeshData => MeshData.ItemName == itemResName).Select(mesh => mesh.MeshItem).FirstOrDefault();

        if (mesh != null)
        {
            return mesh;
        }

        return null;
    }

    public static ArrayMeshDataObject GetArrayMeshDataObject(string itemResName)
    {
        var meshDataObject = arrayMeshDataObjects.Where(MeshData => MeshData.ItemName == itemResName).FirstOrDefault();

        if (meshDataObject != null)
        {
            return meshDataObject;
        }

        return null;
    }

    public static void UpdateMeshScene(BoneAttachment3D parentNode, string scenePath)
    {

        foreach (Node child in parentNode.GetChildren())
        {
            parentNode.RemoveChild(child);
            child.QueueFree();
        }

        Node newScene = GD.Load<PackedScene>(scenePath).Instantiate();
        parentNode.AddChild(newScene);

    }

    public static void UpdateUIOptionsSceneItemList(OptionButton buttonToUpdate, string scenesFolderPath)
    {
        string[] itemList = ResourceLoader.ListDirectory(scenesFolderPath);
        int itemId = 1;
        foreach (string item in itemList)
        {
            if (item.EndsWith(".tscn"))
            {
                int stringIndex = item.IndexOf(".tscn");
                string itemName = item.Substring(0, stringIndex);
                buttonToUpdate.AddItem(itemName, itemId);
                int itemIndex = buttonToUpdate.GetItemIndex(itemId);
                string iconPath = scenesFolderPath + itemName + "_Icon.png";
                Texture2D icon = GD.Load<Texture2D>(iconPath);

                if (icon != null)
                {
                    // Resize the icon to a smaller size
                    Image iconImage = icon.GetImage();
                    iconImage.Resize(_iconSize, _iconSize); // Set the desired size here
                    Texture2D resizedIcon = ImageTexture.CreateFromImage(iconImage);

                    buttonToUpdate.SetItemIcon(itemIndex, resizedIcon);
                }
                itemId++;

            }

        }


    }

}
