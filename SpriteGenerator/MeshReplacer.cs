using Godot;
using System.Collections.Generic;
using System.Linq;
public partial class MeshReplacer : Node
{

    private static int _iconSize = 32;
    public static List<ArrayMeshDataObject> arrayMeshDataObjects;


    public static void UpdateUIOptionMesheItemList(MeshReplacerOptButton itemMeshOptBtn, Const.BodyPartType bodyPart)
    {
        GetAllMeshItemRes();

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

    public static void UpdateMeshFromResourceItem(MeshInstance3D meshTargetToReplace, string resourceitemName)
    {
        meshTargetToReplace.Mesh = GetMeshItemResByName(resourceitemName);
    }

    public static Mesh GetMeshItemResByName(string itemResName)
    {
        var mesh = arrayMeshDataObjects.Where(MeshData => MeshData.ItemName == itemResName).Select(mesh => mesh.MeshItem).FirstOrDefault();

        if (mesh != null)
        {
            return mesh;
        }

        return null;
    }

    public static void GetAllMeshItemRes()
    {
        var allMeshItems = ResourceLoader.ListDirectory(Const.MESH_REPO_FOLDER_PATH);
        arrayMeshDataObjects = new List<ArrayMeshDataObject>();


        foreach (string itemName in allMeshItems)
        {
            string itemPath = Const.MESH_REPO_FOLDER_PATH + itemName;
            if (itemPath.EndsWith(".res") || itemPath.EndsWith(".tres"))
            {
                var meshItem = GD.Load<Resource>(itemPath);
                if (meshItem is ArrayMeshDataObject meshItemObject)
                {
                    arrayMeshDataObjects.Add(meshItemObject);
                }
            }
        }

        arrayMeshDataObjects.Sort((a, b) => a.ItemOrder.CompareTo(b.ItemOrder));

    }

    public static void UpdateHairScene(BoneAttachment3D hairParentNode, string hairScenePath)
    {

        foreach (Node child in hairParentNode.GetChildren())
        {
            hairParentNode.RemoveChild(child);
        }

        Node hairScene = GD.Load<PackedScene>(hairScenePath).Instantiate();
        hairParentNode.AddChild(hairScene);

    }

    public static void UpdateUIOptionsHairList(OptionButton buttonToUpdate)
    {
        string[] hairItemList = ResourceLoader.ListDirectory(Const.HAIR_MESHES_FOLDER_PATH);
        int itemId = 1;
        foreach (string item in hairItemList)
        {
            if (item.EndsWith(".tscn"))
            {
                int stringIndex = item.IndexOf(".tscn");
                string itemName = item.Substring(0, stringIndex);
                buttonToUpdate.AddItem(itemName, itemId);
                int itemIndex = buttonToUpdate.GetItemIndex(itemId);
                string iconPath = Const.HAIR_MESHES_FOLDER_PATH + itemName + "_Icon.png";
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
