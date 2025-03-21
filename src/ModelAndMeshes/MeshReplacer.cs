using System.Collections.Generic;
using System.Linq;
using Godot;
public partial class MeshReplacer : Node
{

    private static int _iconSize = 32;
    public static List<ArrayMeshDataObject> arrayMeshDataObjects;


    public static void UpdateUIOptionMesheItemList(MeshReplacerOptButton itemMeshOptBtn, Const.BodyPartType bodyPart)
    {
        //GetAllMeshItemRes();
        arrayMeshDataObjects = GlobalUtil.GetResourcesByType<ArrayMeshDataObject>(Const.MESH_REPO_FOLDER_PATH);
        arrayMeshDataObjects.Sort((a, b) => a.ItemOrder.CompareTo(b.ItemOrder));

        var bodyPartMeshes = arrayMeshDataObjects.Where(mesh => mesh.BodyPartType == itemMeshOptBtn.BodyPartType).ToList();

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

    public static Mesh GetMeshItemByName(string itemResName, List<ArrayMeshDataObject> resourceMeshList)
    {
        var mesh = resourceMeshList.Where(MeshData => MeshData.ItemName == itemResName).Select(mesh => mesh.MeshItem).FirstOrDefault();

        if (mesh != null)
        {
            return mesh;
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
