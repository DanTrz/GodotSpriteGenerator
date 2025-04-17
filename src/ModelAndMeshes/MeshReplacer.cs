using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
public static class MeshReplacer
{

    private static int _iconSize = 32;
    public static List<ArrayMeshDataObject> arrayMeshDataObjects;

    static MeshReplacer()
    {
        LoadAllMeshDataResouces();
        GlobalEvents.Instance.OnMeshItemColorChanged += UpdateMeshItemColor;
    }


    private static void LoadAllMeshDataResouces()
    {
        //GetAllMeshItem data that are ArrayMeshDataObject type and load to a local list
        arrayMeshDataObjects = GlobalUtil.GetAndLoadResourcesByType<ArrayMeshDataObject>(Const.MESH_REPO_FOLDER_PATH);
        arrayMeshDataObjects.Sort((a, b) => a.ItemOrder.CompareTo(b.ItemOrder));
    }

    public static void UpdateMeshItemColor(string itemSelectedName, Const.BodyPartType bodyPart, Color newColor)
    {
        var meshItem = GetMeshItemByNameFromList(itemSelectedName, bodyPart, arrayMeshDataObjects);
        if (meshItem.SurfaceGetMaterial(0) is StandardMaterial3D material)
        {
            material.AlbedoColor = newColor;

        }
    }

    public static void UpdateUIOptionMesheItemList(MeshReplacerOptButton itemMeshOptBtn)
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

    public static void UpdateMeshFromResourceItem(MeshInstance3D meshTargetToReplace, string resourceItemName, Const.BodyPartType bodyPartType)
    {

        if (meshTargetToReplace == null || meshTargetToReplace.Mesh == null)
        {
            Log.Error(meshTargetToReplace.Name + " has no mesh or is null");
        }

        meshTargetToReplace.Mesh = GetMeshItemByNameFromList(resourceItemName, bodyPartType, arrayMeshDataObjects);

    }

    public static Mesh GetMeshItemByNameFromList(string itemResName, Const.BodyPartType bodyPartType, List<ArrayMeshDataObject> resourceMeshList)
    {
        var mesh = resourceMeshList.Where(MeshData => MeshData.ItemName == itemResName && MeshData.BodyPartType == bodyPartType).
        Select(mesh => mesh.MeshItem).FirstOrDefault();

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

    // public static Aabb GetModelMergedAABBMeshes(Node parentNode)
    // {
    //     var allMeshesbjects = GlobalUtil.GetAllChildNodesByType<MeshInstance3D>(parentNode).Select(mesh => mesh.Mesh).ToList();

    //     Aabb mergedAABB = new Aabb();
    //     foreach (Mesh mesh in allMeshesbjects)
    //     {
    //         Aabb meshAABBounds = mesh.GetAabb();
    //         mergedAABB.Merge(with: meshAABBounds);
    //     }
    //     Log.Debug("Merged AABB: " + mergedAABB.Size);

    //     return mergedAABB;
    // }

    /// <summary>
    /// Calculates a merged Axis-Aligned Bounding Box (AABB) that encloses all
    /// MeshInstance3D nodes found as descendants of the given parentNode.
    /// The AABB accounts for the global transform (position, rotation, scale) of each mesh instance.
    /// </summary>
    /// <param name="parentNode">The parent node to search under.</param>
    /// <returns>The merged AABB in global coordinates. Returns an empty AABB at origin if no meshes are found or if none have a valid volume.</returns>
    public static Aabb GetModelMergedAABBMeshes(Node parentNode)
    {

        // var allMeshInstances = GlobalUtil.GetAllChildNodesByType<MeshInstance3D>(parentNode);
        var allMeshInstances = GlobalUtil.GetAllChildNodesByType<MeshInstance3D>(parentNode);

        if (allMeshInstances.Count == 0)
        {
            // Log.Debug($"No MeshInstance3D nodes found under {parentNode.Name}");
            return new Aabb(Vector3.Zero, Vector3.Zero);
        }

        Aabb finalMergedAabb = new Aabb();
        bool firstAabbFound = false;

        foreach (MeshInstance3D meshInstance in allMeshInstances)
        {
            //TODO : //BUG = Not properly working the Scaling logic
            // Log.Debug($"Transform for {meshInstance.Name}: Scale={meshInstance.Scale} " +
            //     $"Parent={meshInstance.GetParent<Node3D>().Name} ParentScale={meshInstance.GetParent<Node3D>().Scale}" +
            //     $"Grand Parent={meshInstance.GetParent().GetParent<Node3D>().Name} GrandPar Scale={meshInstance.GetParent().GetParent<Node3D>().Scale}");

            // Get local AABB (relative to node origin, includes node scale)
            Aabb localAabb = meshInstance.GetAabb();
            if (!localAabb.HasVolume()) // Skip if mesh/node scale results in no volume
            {
                continue;
            }

            // Get the node's global transform
            Transform3D globalTransform = meshInstance.GlobalTransform;

            // --- Manual AABB Transformation (Required for Godot 4.x C#) ---
            Aabb instanceGlobalAabb;
            try
            {
                // 1. Calculate the 8 corner points of the local AABB
                Vector3 pos = localAabb.Position;
                Vector3 size = localAabb.Size;
                Span<Vector3> corners = [
                    pos,
                    pos + new Vector3(size.X, 0, 0),
                    pos + new Vector3(0, size.Y, 0),
                    pos + new Vector3(0, 0, size.Z),
                    pos + new Vector3(size.X, size.Y, 0),
                    pos + new Vector3(size.X, 0, size.Z),
                    pos + new Vector3(0, size.Y, size.Z),
                    pos + size // Max corner (Position + Size)
                ];

                // 2. Transform the first corner using the multiplication operator
                Vector3 firstTransformedCorner = globalTransform * corners[0];

                instanceGlobalAabb = new Aabb(firstTransformedCorner, Vector3.Zero);

                // 3. Transform the remaining 7 corners and expand the global AABB
                for (int i = 1; i < 8; i++)
                {
                    Vector3 transformedCorner = globalTransform * corners[i];
                    instanceGlobalAabb = instanceGlobalAabb.Expand(transformedCorner);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error transforming AABB corners for node '{meshInstance.Name}': {ex.Message}");
                continue; // Skip this instance if transformation fails
            }

            // 4. Merge this manually calculated global AABB into the final result
            if (!firstAabbFound)
            {
                finalMergedAabb = instanceGlobalAabb;
                firstAabbFound = true;
            }
            else
            {
                finalMergedAabb = finalMergedAabb.Merge(instanceGlobalAabb);
            }
        }

        if (!firstAabbFound)
        {
            //Log.Debug($"No MeshInstance3D nodes with valid AABBs found under {parentNode.Name}");
            return new Aabb(Vector3.Zero, Vector3.Zero);
        }

        //Apply some foreced corrections if the model is too small or has weird scale (not 1.0 Scale)
        if (finalMergedAabb.Size.X < 0.5f || finalMergedAabb.Size.Y < 0.5f || finalMergedAabb.Size.Z < 0.5f)
        {
            if (finalMergedAabb.Size.X > 0.1f || finalMergedAabb.Size.Y > 0.1f || finalMergedAabb.Size.Z > 0.1f)
            {
                {
                    finalMergedAabb = new Aabb(finalMergedAabb.Position,
                                    new Vector3(finalMergedAabb.Size.X * 10, finalMergedAabb.Size.Y * 10, finalMergedAabb.Size.Z * 10));

                }
            }
            else
            {
                finalMergedAabb = new Aabb(finalMergedAabb.Position,
                                   new Vector3(finalMergedAabb.Size.X * 50000, finalMergedAabb.Size.Y * 50000, finalMergedAabb.Size.Z * 50000));

            }
        }

        Log.Debug($"Merged AABB for '{parentNode.GetChild(0).Name}': Pos={finalMergedAabb.Position}, Size={finalMergedAabb.Size}");
        return finalMergedAabb;
    }

    private static void CorrectAllChildNodeScales(Node parentNode)
    {
        var allChildNode3D = parentNode.GetChild(0).GetChildren();

        foreach (var node in allChildNode3D)
        {
            if (node is Node3D myNode3D)
            {
                Vector3 originalScale = myNode3D.Scale;

                if (originalScale != Vector3.One)
                {
                    //Log.Debug($"Correcting scale for {myNode3D.Name} from {originalScale} to Vector3.One");
                    myNode3D.Scale = Vector3.One;
                }
            }

        }



    }
}


