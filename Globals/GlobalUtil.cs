using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public static class GlobalUtil
{

    // --- Folder Functions ---
    #region FOLDER ACCESS:  Helper Functions

    private static string _saveFolder = Const.RES_TEMPSAVE_FOLDER_PATH;
    public static string SaveFolderPath
    {
        get { return _saveFolder; }
        set { _saveFolder = value; }
    }

    public static async Task<bool> HasDirectory(string path, Node sourceNode)
    {
        if (Directory.Exists(path))
        {
            return true;
        }
        else
        {
            GD.PrintErr("Directory does not exist: " + path);

            await Task.Run(() =>
            {
                ShowErrorDialog("Error: Directory not Found", "Directory does not exist: " + path, sourceNode);
            });


            return false;
        }
    }

    public static void ShowErrorDialog(string title, string message, Node parentNode)
    {
        using Godot.AcceptDialog acceptDialog = new Godot.AcceptDialog
        {
            Title = message,
            DialogText = message
        };

        parentNode.AddChild(acceptDialog);
        acceptDialog.PopupCentered();

    }

    public static void OnFolderSelected(string dir, LineEdit spriteGenFolderPathLineEdit = null)
    {
        GD.PrintT("OnFolderSelected: " + dir);
        SaveFolderPath = dir;
        spriteGenFolderPathLineEdit.Text = GlobalUtil.SaveFolderPath;
    }


    #endregion FOLDER ACCESS:  Helper Functions

    // --- Image Functions ---
    #region IMAGE Processing:  Helper Functions

    public static List<Color> KMeansClustering(Image image, int k, SpinBox colorCountSpinBox)
    {
        if (colorCountSpinBox.Value == 0) { return new List<Color> { new Color(0, 0, 0, 0) }; }

        List<Color> colors = new List<Color>();
        int width = image.GetWidth();
        int height = image.GetHeight();
        byte[] data = image.GetData();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = (y * width + x) * 4;
                if (data[index + 3] > 0)
                {
                    colors.Add(GetColorFromBytes(data, index));
                }
            }
        }

        if (colors.Count == 0) return new List<Color>() { new Color(0, 0, 0, 0) };

        List<Color> centroids = new List<Color>();
        Random random = new Random();
        for (int i = 0; i < k; i++) centroids.Add(colors[random.Next(colors.Count)]);

        // --- Limit iterations and optimize cluster assignment ---
        int maxIterations = 16;
        List<Color>[] clusters = new List<Color>[k]; // Initialize clusters *outside* the main loop
        for (int i = 0; i < k; i++) clusters[i] = new List<Color>();

        for (int iter = 0; iter < maxIterations; iter++)
        {
            // Clear clusters at the *beginning* of each iteration.  Crucial for correctness.
            for (int i = 0; i < k; i++)
            {
                clusters[i].Clear();
            }

            // Assign colors to clusters (more efficient using index)
            foreach (Color color in colors)
            {
                int nearestCentroidIndex = FindNearestCentroidIndex(color, centroids);
                clusters[nearestCentroidIndex].Add(color);
            }

            // Update centroids
            List<Color> newCentroids = new List<Color>();
            for (int i = 0; i < k; i++)
            {
                newCentroids.Add(clusters[i].Count > 0 ? CalculateMeanColor(clusters[i]) : colors[random.Next(colors.Count)]);
            }
            centroids = newCentroids;
        }
        return centroids;
    }

    public static int FindNearestCentroidIndex(Color color, List<Color> centroids)
    {
        int nearestIndex = 0;
        float minDistanceSquared = float.MaxValue; // Use squared distance

        for (int i = 0; i < centroids.Count; i++)
        {
            // Calculate squared distance (avoiding square root)
            float distanceSquared = ColorDistanceSquared(color, centroids[i]);
            if (distanceSquared < minDistanceSquared)
            {
                minDistanceSquared = distanceSquared;
                nearestIndex = i;
            }
        }
        return nearestIndex;
    }

    // Calculate squared color distance
    public static float ColorDistanceSquared(Color c1, Color c2)
    {
        float dr = c1.R - c2.R;
        float dg = c1.G - c2.G;
        float db = c1.B - c2.B;
        float da = c1.A - c2.A;
        return dr * dr + dg * dg + db * db + da * da; // No square root!
    }

    public static Color CalculateMeanColor(List<Color> colors)
    {
        float sumR = 0, sumG = 0, sumB = 0, sumA = 0;
        foreach (Color color in colors)
        {
            sumR += color.R; sumG += color.G; sumB += color.B; sumA += color.A;
        }
        int count = colors.Count;
        return new Color(sumR / count, sumG / count, sumB / count, sumA / count);
    }

    public static Color FindClosestColor(Color targetColor, List<Color> palette)
    {
        Color closestColor = palette[0];
        float minDistanceSquared = ColorDistanceSquared(targetColor, closestColor); // Use squared distance

        foreach (Color paletteColor in palette)
        {
            float distanceSquared = ColorDistanceSquared(targetColor, paletteColor); // Use squared distance
            if (distanceSquared < minDistanceSquared)
            {
                minDistanceSquared = distanceSquared;
                closestColor = paletteColor;
            }
        }
        return closestColor;
    }
    //private Color GetColorFromBytes(byte[] data, int index) => new Color(data[index] / 255.0f, data[index + 1] / 255.0f, data[index + 2] / 255.0f, data[index + 3] / 255.0f);

    public static Color GetColorFromBytes(byte[] data, int index)
    {
        if (index + 3 >= data.Length)
        {
            throw new IndexOutOfRangeException("Index exceeds the bounds of the data array.");
        }

        return new Color(data[index] / 255.0f, data[index + 1] / 255.0f, data[index + 2] / 255.0f, data[index + 3] / 255.0f);
    }

    public static void SetColorToBytes(byte[] data, int index, Color color)
    {
        data[index] = (byte)(color.R * 255);
        data[index + 1] = (byte)(color.G * 255);
        data[index + 2] = (byte)(color.B * 255);
        data[index + 3] = (byte)(color.A * 255);
    }

    public static int GetTotalColorCount(Image image)
    {
        if (image.IsEmpty())
        {
            return 0;
        }

        HashSet<ulong> uniqueColors = new HashSet<ulong>(); // Use ulong for color comparison

        for (int x = 0; x < image.GetWidth(); x++)
        {
            for (int y = 0; y < image.GetHeight(); y++)
            {
                Color color = image.GetPixel(x, y);
                // Convert Color to a single ulong for efficient comparison
                //ulong colorValue = ((ulong)(color.R8) << 24) | ((ulong)(color.G8) << 16) | ((ulong)(color.B8) << 8) | (ulong)(color.A8);

                ulong colorValue = ((uint)color.R8 << 24) | ((uint)color.G8 << 16) | ((uint)color.B8 << 8) | (uint)color.A8;
                uniqueColors.Add(colorValue);
            }
        }
        return uniqueColors.Count;
    }

    public static Color FindClosestPaletteColor(Color color) => new Color(Mathf.Round(color.R * 255) / 255, Mathf.Round(color.G * 255) / 255, Mathf.Round(color.B * 255) / 255, color.A);
    public static Color ClampColor(Color color) => new Color(Mathf.Clamp(color.R, 0, 1), Mathf.Clamp(color.G, 0, 1), Mathf.Clamp(color.B, 0, 1), color.A);

    public static float ClampFloat(float value, float min, float max)
    {
        return Mathf.Clamp(value, min, max);
    }

    public static Godot.Collections.Array<Color> GetGodotArrayFromColorList(List<Color> colorList)
    {
        //1. Convert the colorList to a Godot.Array
        Godot.Collections.Array<Color> godotArray = new();
        foreach (Color color in colorList)
        {
            godotArray.Add(color);
        }
        return godotArray;
    }

    public static Godot.Collections.Array GetGodotArrayFromGenericList<T>(List<T> genericList)
    {
        //1. Convert the colorList to a Godot.Array
        Godot.Collections.Array godotArray = new();
        foreach (var item in genericList)
        {
            godotArray.Add(item as GodotObject);
        }
        return godotArray;
    }

    public static List<Color> GetListFromGodotArray(Godot.Collections.Array<Color> colorList)
    {
        //1. Convert the colorList to a Godot.Array
        List<Color> cSharpList = new();
        foreach (Color color in colorList)
        {
            cSharpList.Add(color);
        }
        return cSharpList;
    }


    #endregion IMAGE Processing:  Helper Functions


    #region NODE Processing:  Helper Functions


    /// <summary>
    /// Returns all Nodes as a List that matches the type T. Recursive search from the Parent Node "fromParentNode"
    /// Returns all children, grandchildren, etc.
    /// </summary>
    public static List<T> GetAllNodesByType<T>(Node fromParentNode) where T : Node
    {
        List<T> nodesFound = new List<T>();

        // Check the current node
        if (fromParentNode is T typedNode)
        {
            nodesFound.Add(typedNode);
        }

        // Recursively check all children
        foreach (Node child in fromParentNode.GetChildren())
        {
            nodesFound.AddRange(GetAllNodesByType<T>(child));
        }

        return nodesFound;
    }

    public static List<T> GetAndLoadResourcesByType<T>(string resourceDirPath) where T : Resource
    {
        var allResourceFiles = ResourceLoader.ListDirectory(resourceDirPath);
        List<T> resourceList = new List<T>();


        foreach (string itemName in allResourceFiles)
        {
            string itemFullPath = resourceDirPath + itemName;
            if (itemFullPath.EndsWith(".res") || itemFullPath.EndsWith(".tres"))
            {
                var resourceItem = GD.Load<Resource>(itemFullPath);
                if (resourceItem is T myResource)
                {
                    resourceList.Add(myResource);
                }
            }
        }

        return resourceList;
    }

    public static List<T> GetResourceByNameFromList<T>(string itemResName, List<T> resourcceList) where T : Resource
    {
        var resourceItem = resourcceList.Where(item => item.ResourceName == itemResName).FirstOrDefault();

        if (resourceItem != null)
        {
            return resourceItem as GodotObject as List<T>;
        }

        return null;
    }

    public static List<T> GetAllResourcesByTypeFromDisk<T>(string resourceDirPath, T resourceType) where T : Resource
    {
        var allResouces = ResourceLoader.ListDirectory(resourceDirPath);
        List<T> filteredResouces = new List<T>();

        foreach (string item in allResouces)
        {
            string itemFullPath = resourceDirPath + item;
            if (itemFullPath.EndsWith(".res") || itemFullPath.EndsWith(".tres"))
            {
                var resourceItem = GD.Load<Resource>(itemFullPath);
                if (resourceItem is T myResource)
                {
                    filteredResouces.Add(myResource);
                }
            }
        }
        return filteredResouces;
    }

    public static List<T> GetResourceByNameFromDisk<T>(string itemResName, string resourceDirPath) where T : Resource
    {

        var allResourceFiles = GetAllResourcesByTypeFromDisk<Resource>(resourceDirPath, default(T));
        var resourceItem = allResourceFiles.Where(item => item.ResourceName == itemResName).FirstOrDefault();

        if (resourceItem != null)

        {
            return resourceItem as GodotObject as List<T>;
        }

        return null;
    }

    public static void PrintActionTargetListeners<T>(Action<T> actionToCheck, string actionName)
    {
        var targetList = actionToCheck.GetInvocationList().ToList().Where(action => action.Target != null).Select(action => action.Target).ToList();
        // string actionName = actionToCheck.Method.Name;
        // string actionName2 = nameof(actionToCheck);
        GD.PrintT("## PRINT Action: " + actionName + " => Method connected: " + actionToCheck.Method.Name);

        foreach (var target in targetList)
        {
            GD.PrintT("## PRINT Action: " + actionName + " => Listener Target: " + target.ToString());

            if (target is GodotObject godotTarget)
            {
                var type = godotTarget.GetType();
                GD.PrintT("## PRINT Action: " + actionName + " => Listener Godot Type: " + type);

                if (target is Node nodeTarget) // Check if it's a Node or whateverElse
                {
                    GD.PrintT("## PRINT Action: " + actionName + " => Listener Node Name: " + nodeTarget.Name);
                    GD.PrintT("## PRINT Action: " + actionName + " => Listener Node Path: " + nodeTarget.GetPath());
                }
            }
        }

    }


    #endregion NODE Processing:  Helper Functions

}
