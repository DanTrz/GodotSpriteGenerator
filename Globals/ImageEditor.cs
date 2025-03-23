using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Godot;

public partial class ImageEditor : PanelContainer
{
    //[ExportToolButton("UpdateShader")] public Callable ClickMeButton => Callable.From(UpdateShaderParameters);
    [Export] public TextureRect ImgTextRect;
    private Texture2D currentTexture;
    [Export] public bool EnableColorReduction = false;
    [Export] public int NumColors = 16; //Value of the colors applied by the Shader. Usually what's in the SpinBox Color Reduc

    [Export] public int MaxNumColors = 256; // Dynamic max palette size. This value might change if we add Persistent Colors. 

    public int OriginalNumColors = 0; //StaticValue that should NEVER change after loading an image
    public Godot.Collections.Array<Color> OriginalImgPalette = new();


    //[Export] public bool UseExternalPalette = false;
    //public List<Color> ShaderPalette = new();
    [Export] public bool EnableSaturation = true;
    [Export] public float SaturationValue = 1.0f;
    [Export] public bool EnableBrightness = true;
    [Export] public float BrightnessValue = 0.0f;

    [Export] public bool EnableContrast = true;
    [Export] public float ConstrastValue = 1.0f;
    [Export] public SubViewport ImgEditorSubViewport;

    //[Export] public Godot.Collections.Array<Color> _currentPaletteColors = new(); //public List<Color> _currentPaletteColors = new(); Brightness = 1.0f;
    [Export] public Godot.Collections.Array<Color> ShaderPalette = new();

    //HD Section to Saving
    [Export] public CanvasLayer HDCanvasLayer;
    [Export] public SubViewportContainer HDSubViewportContainer;
    [Export] public SubViewport HDSubviewPort;
    [Export] public TextureRect HDTextureRect;
    [Export] public TextureRect HDTempBGTextureRect;

    public bool _useExternalPalette = false;

    public List<Color> ImgkMeansClusterList = new();

    public readonly int MAX_PALETTE_SIZE = 512;


    public override void _Ready()
    {
        currentTexture = ImgTextRect.Texture;
        NumColors = GetColorFrequencies(currentTexture.GetImage()).Count();
        //NumColors = GetUniqueColorsCount(currentTexture.GetImage()).Result; // this Updates NumColors variaable
        //HDCanvasLayer.Visible = false;
    }

    public void UpdateShaderParameters()
    {
        //Texture currentTexture = ImgTextRect.Texture;

        if (ImgTextRect.Material is not ShaderMaterial shaderMaterial)
        {
            GD.PrintErr("Material is not a ShaderMaterial.");
            return;
        }

        shaderMaterial.SetShaderParameter("saturation", SaturationValue);
        shaderMaterial.SetShaderParameter("brightness", BrightnessValue);
        shaderMaterial.SetShaderParameter("contrast", ConstrastValue);


        shaderMaterial.SetShaderParameter("enable_color_reduction", EnableColorReduction);
        shaderMaterial.SetShaderParameter("num_colors", NumColors);
        //shaderMaterial.SetShaderParameter("use_external_palette", UseExternalPalette);

        if (!EnableColorReduction) return;

        // if (!_useExternalPalette)
        // {
        //     currentTexture = ImgTextRect.Texture;

        //     //TODO: Determine if this is needed #BUG
        //     ShaderPalette = GetOriginalTexturePalette();
        // }

        shaderMaterial.SetShaderParameter("palette", ShaderPalette);

    }

    /// <summary>
    /// Reads the original image loaded into the main Image TextureRect and returns a unique list of colors.
    /// Uses the KMeansClustering algorithm to get a new color list that respects the most frequent colors in the original image.
    /// Optional: you can add a color list to be merged via the "additionalColors" parameter (Only new colors will be added).
    /// </summary>
    /// <param name="colorsToGet">The max number of colors to get.</param>
    /// <param name="additionalColors">Add new colors if they don't exist</param>
    /// <returns>Returns a new Godot Array with the colors.</returns>
    public async Task<Godot.Collections.Array<Color>> GetNewColorPalette(int colorsToGet)
    {
        currentTexture = ImgTextRect.Texture;

        if (currentTexture is Texture2D texture2D)
        {
            Image image = texture2D.GetImage();


            if (image.GetSize() == Vector2.Zero)
            {
                GD.PrintErr("Texture has zero size, dynamic palette generation will not work.");
                return new Godot.Collections.Array<Color> { Colors.Black };
            }
            else
            {

                //List<Color> originalTexturePalette = KMeansClustering(image, NumColors).Result;
                //return ColorListToGodotArray(originalTexturePalette);
                // int uniqueColorsGodot = ColorListToGodotArray(originalTexturePalette.Distinct().ToList()).Count();
                // GD.Print("Unique Colors after GD Conversion = " + uniqueColorsGodot);

                //CallDeferred("UpdateKMeansClusteringList", image, NumColors);
                await UpdatedKMeansClusteringAsync(image, colorsToGet);

                List<Color> originalTexturePalette = ImgkMeansClusterList;


                // if (additionalColors != null)
                // {
                //     List<Color> colorsToAdd = additionalColors.Where(color => !originalTexturePalette.Contains(color)).Distinct().ToList();
                //     GD.PrintT("Colors to add to palette = " + colorsToAdd.Count);

                //     originalTexturePalette.AddRange(colorsToAdd);
                //     //originalTexturePalette.AddRange(additionalColors);
                //     GD.PrintT("Unified Palette = " + originalTexturePalette.Count);
                // }

                //int uniqueColorsGodot = ColorListToGodotArray(originalTexturePalette.Distinct().ToList()).Count();
                //GD.Print("Unique Colors after GD Conversion = " + uniqueColorsGodot);

                return GlobalUtil.GetGodotArrayFromList(originalTexturePalette.Distinct().ToList());

            }
        }
        GD.PrintErr("Cannot get original texture palette. Texture is null or not a Texture2D.");
        return new Godot.Collections.Array<Color> { Colors.Black };

    }


    /// <summary>
    /// Iterate over the pixels in the image and count the number of times each color appears, up to a maximum of colors = colorsK.
    /// Works by identifying the color that are more frequently present in the image.
    /// Useful if we need to get a new color list that respects the most frequent colors in the original image.
    /// </summary>
    /// <param name="colorsK">The max number of colors to get.</param>
    /// <returns>Returns the Colors that are more frequently present in the image, sorted by most frequent to least frequent..</returns>
    public async Task<List<Color>> UpdatedKMeansClusteringAsync(Image image, int colorsK)
    {
        GD.Print("KMeansClustering Async: Max Colors to check = " + colorsK);

        await Task.Run(() =>
        {
            if (image == null || image.GetWidth() == 0 || image.GetHeight() == 0)
            {
                ImgkMeansClusterList = Enumerable.Repeat(Colors.Black, colorsK).ToList();
            }

            // 1. Get *all* colors (not just unique) and their frequencies.
            Dictionary<Color, int> colorFrequencies = GetColorFrequencies(image);

            // 2. Handle cases where the number of unique colors is less than k.
            if (colorFrequencies.Count <= colorsK)
            {
                List<Color> result = colorFrequencies.Keys.ToList();
                while (result.Count < colorsK)
                {
                    result.Add(Colors.Black); // Pad with black
                }
                ImgkMeansClusterList = result;
            }


            // 3.  Initialization:  A hybrid approach.
            List<Color> centroids = new();
            // 3a.  Prioritize the MOST FREQUENT colors.
            List<Color> sortedColors = colorFrequencies.OrderByDescending(pair => pair.Value).Select(pair => pair.Key).ToList();
            centroids.AddRange(sortedColors.Take(Math.Min(colorsK, sortedColors.Count))); // Add the top 'k' (or fewer) most frequent

            // 3b. If we still need more centroids (unlikely, but possible), fill the rest randomly.
            if (centroids.Count < colorsK)
            {
                HashSet<Color> uniqueColors = new HashSet<Color>(colorFrequencies.Keys);
                centroids.AddRange(uniqueColors.OrderBy(_ => GD.Randi()).Take(colorsK - centroids.Count));
            }

            // 4.  Iteration (rest of the K-Means algorithm remains the same).
            int maxIterations = 100;
            List<Color>[] clusters = new List<Color>[colorsK];
            for (int i = 0; i < colorsK; i++) clusters[i] = new();

            for (int iteration = 0; iteration < maxIterations; iteration++)
            {
                for (int i = 0; i < colorsK; i++) clusters[i].Clear();

                foreach (Color color in colorFrequencies.Keys) // Iterate over unique colors
                {
                    int nearestCentroidIndex = FindNearestCentroidIndex(color, centroids);
                    // Add the color to the cluster, weighted by its frequency.  This is important!
                    for (int i = 0; i < colorFrequencies[color]; i++)
                    {
                        clusters[nearestCentroidIndex].Add(color);
                    }
                }

                List<Color> newCentroids = new();
                bool centroidsChanged = false;

                for (int i = 0; i < colorsK; i++)
                {
                    Color newCentroid = clusters[i].Count > 0 ? CalculateMeanColor(clusters[i]) : centroids[i];
                    if (!newCentroid.IsEqualApprox(centroids[i]))
                    {
                        centroidsChanged = true;
                    }
                    newCentroids.Add(newCentroid);
                }

                centroids = newCentroids;

                if (!centroidsChanged)
                {
                    break;
                }
            }
            ImgkMeansClusterList = centroids;


        });

        GD.Print("KMeansClusteringAsync Completed with colors = " + ImgkMeansClusterList.Count);
        //GlobalEvents.Instance.OnEffectsChangesEnded.Invoke(this.Name, ColorListToGodotArray(ImgkMeansClusterList));

        return ImgkMeansClusterList;
    }

    public void UpdatedKMeansClusteringSyncronous(Image image, int colorsK)
    {
        GD.Print("KMeansClustering Syncronous: Max Colors to check = " + colorsK);

        // await Task.Run(() =>
        // {
        if (image == null || image.GetWidth() == 0 || image.GetHeight() == 0)
        {
            ImgkMeansClusterList = Enumerable.Repeat(Colors.Black, colorsK).ToList();
        }

        // 1. Get *all* colors (not just unique) and their frequencies.
        Dictionary<Color, int> colorFrequencies = GetColorFrequencies(image);

        // 2. Handle cases where the number of unique colors is less than k.
        if (colorFrequencies.Count <= colorsK)
        {
            List<Color> result = colorFrequencies.Keys.ToList();
            while (result.Count < colorsK)
            {
                result.Add(Colors.Black); // Pad with black
            }
            ImgkMeansClusterList = result;
        }


        // 3.  Initialization:  A hybrid approach.
        List<Color> centroids = new();
        // 3a.  Prioritize the MOST FREQUENT colors.
        List<Color> sortedColors = colorFrequencies.OrderByDescending(pair => pair.Value).Select(pair => pair.Key).ToList();
        centroids.AddRange(sortedColors.Take(Math.Min(colorsK, sortedColors.Count))); // Add the top 'k' (or fewer) most frequent

        // 3b. If we still need more centroids (unlikely, but possible), fill the rest randomly.
        if (centroids.Count < colorsK)
        {
            HashSet<Color> uniqueColors = new HashSet<Color>(colorFrequencies.Keys);
            centroids.AddRange(uniqueColors.OrderBy(_ => GD.Randi()).Take(colorsK - centroids.Count));
        }

        // 4.  Iteration (rest of the K-Means algorithm remains the same).
        int maxIterations = 100;
        List<Color>[] clusters = new List<Color>[colorsK];
        for (int i = 0; i < colorsK; i++) clusters[i] = new();

        for (int iteration = 0; iteration < maxIterations; iteration++)
        {
            for (int i = 0; i < colorsK; i++) clusters[i].Clear();

            foreach (Color color in colorFrequencies.Keys) // Iterate over unique colors
            {
                int nearestCentroidIndex = FindNearestCentroidIndex(color, centroids);
                // Add the color to the cluster, weighted by its frequency.  This is important!
                for (int i = 0; i < colorFrequencies[color]; i++)
                {
                    clusters[nearestCentroidIndex].Add(color);
                }
            }

            List<Color> newCentroids = new();
            bool centroidsChanged = false;

            for (int i = 0; i < colorsK; i++)
            {
                Color newCentroid = clusters[i].Count > 0 ? CalculateMeanColor(clusters[i]) : centroids[i];
                if (!newCentroid.IsEqualApprox(centroids[i]))
                {
                    centroidsChanged = true;
                }
                newCentroids.Add(newCentroid);
            }

            centroids = newCentroids;

            if (!centroidsChanged)
            {
                break;
            }
        }

        GD.Print("KMeansClustering Completed with colors = " + ImgkMeansClusterList.Count);
        ImgkMeansClusterList = centroids;

        // });

        //GlobalEvents.Instance.OnEffectsChangesEnded.Invoke(this.Name, ColorListToGodotArray(ImgkMeansClusterList));
    }

    //public async IAsyncEnumerable<Color> GetColorFrequencies(Image image)
    /// <summary>
    /// Counts the number of times each color appears in the image and returns a Dictionary with the colors and and number of times they appear. 
    /// The Keys represent a Unique Color and the Values are a Int that represents the number of times it appears.
    /// </summary>
    /// <returns>Returns a Dictionary with the colors. Key = Color, Value = How many times it appears</returns>
    public Dictionary<Color, int> GetColorFrequencies(Image image)
    {
        Dictionary<Color, int> colorFrequencies = new();

        for (int y = 0; y < image.GetHeight(); y++)
        {
            for (int x = 0; x < image.GetWidth(); x++)
            {
                Color pixelColor = image.GetPixel(x, y);
                if (pixelColor.A > 0) // Consider alpha
                {
                    if (colorFrequencies.ContainsKey(pixelColor))
                        colorFrequencies[pixelColor]++;
                    else
                        colorFrequencies[pixelColor] = 1;
                }
            }
        }
        GD.Print("GetUniqueColorsCount => Result of unique colors = " + colorFrequencies.Count);
        return colorFrequencies;
    }

    // private async void UpdateKMeansClusteringList(Image image, int colorsK)
    // {
    //     GD.PrintT("KMeansClustering: Started. Max colors= " + colorsK);
    //     await Task.Run(() =>
    //     {
    //         if (image == null || image.GetWidth() == 0 || image.GetHeight() == 0)
    //         {
    //             ImgkMeansClusterList = Enumerable.Repeat(Colors.Black, colorsK).ToList();
    //         }

    //         // 1. Get all colors and their frequencies.
    //         Dictionary<Color, int> colorFrequencies = GetColorFrequencies(image);

    //         // Dictionary<Color, int> colorFrequencies = new();
    //         // for (int y = 0; y < image.GetHeight(); y++)
    //         // {
    //         //     for (int x = 0; x < image.GetWidth(); x++)
    //         //     {
    //         //         Color pixelColor = image.GetPixel(x, y);
    //         //         if (pixelColor.A > 0) // Consider alpha
    //         //         {
    //         //             if (colorFrequencies.ContainsKey(pixelColor))
    //         //                 colorFrequencies[pixelColor]++;
    //         //             else
    //         //                 colorFrequencies[pixelColor] = 1;
    //         //         }
    //         //     }
    //         // }

    //         // 2. If unique colors are less than colorsK, pad the result.
    //         GD.PrintT("KMeansClustering: Frequencies of unique colors = " + colorFrequencies.Count);
    //         if (colorFrequencies.Count <= colorsK)
    //         {
    //             List<Color> result = colorFrequencies.Keys.ToList();
    //             while (result.Count < colorsK)
    //             {
    //                 result.Add(Colors.Black); // Pad with black
    //             }
    //             ImgkMeansClusterList = result;
    //         }

    //         // 3. Initialization: Use most frequent colors for deterministic behavior.
    //         List<Color> sortedColors = colorFrequencies
    //             .OrderByDescending(pair => pair.Value)
    //             .Select(pair => pair.Key)
    //             .ToList();
    //         List<Color> centroids = new();
    //         centroids.AddRange(sortedColors.Take(colorsK));

    //         // In case additional centroids are needed, fill them deterministically.
    //         if (centroids.Count < colorsK)
    //         {
    //             centroids.AddRange(sortedColors.Skip(centroids.Count).Take(colorsK - centroids.Count));
    //         }

    //         // 4. K-Means iterations using weighted sums.
    //         int maxIterations = 16;
    //         for (int iteration = 0; iteration < maxIterations; iteration++)
    //         {
    //             // Initialize accumulators for weighted sums and counts.
    //             Vector3[] sumColors = new Vector3[colorsK];
    //             int[] totalWeights = new int[colorsK];

    //             // Assignment: update accumulators instead of creating lists.
    //             foreach (var kvp in colorFrequencies)
    //             {
    //                 Color color = kvp.Key;
    //                 int frequency = kvp.Value;
    //                 int nearestIndex = FindNearestCentroidIndex(color, centroids);
    //                 sumColors[nearestIndex] += new Vector3(color.R, color.G, color.B) * frequency;
    //                 totalWeights[nearestIndex] += frequency;
    //             }

    //             bool centroidsChanged = false;
    //             List<Color> newCentroids = new List<Color>(colorsK);

    //             // Update centroids based on weighted sums.
    //             for (int i = 0; i < colorsK; i++)
    //             {
    //                 Color newCentroid;
    //                 if (totalWeights[i] > 0)
    //                 {
    //                     float r = sumColors[i].X / totalWeights[i];
    //                     float g = sumColors[i].Y / totalWeights[i];
    //                     float b = sumColors[i].Z / totalWeights[i];
    //                     newCentroid = new Color(r, g, b, 1.0f);
    //                 }
    //                 else
    //                 {
    //                     // If no color was assigned, keep the previous centroid.
    //                     newCentroid = centroids[i];
    //                 }

    //                 if (!newCentroid.IsEqualApprox(centroids[i]))
    //                     centroidsChanged = true;

    //                 newCentroids.Add(newCentroid);
    //             }

    //             centroids = newCentroids;

    //             if (!centroidsChanged)
    //                 break;
    //         }
    //         ImgkMeansClusterList = centroids;

    //     });


    // }


    private static int FindNearestCentroidIndex(Color color, List<Color> centroids)
    {
        int nearestIndex = 0;
        float minDistance = float.MaxValue;

        for (int i = 0; i < centroids.Count; i++)
        {
            float distance = ColorDistance(color, centroids[i]);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestIndex = i;
            }
        }
        return nearestIndex;
    }

    private static Color CalculateMeanColor(List<Color> cluster)
    {
        if (cluster.Count == 0) return Colors.Black;

        Vector4 sum = Vector4.Zero;
        foreach (Color color in cluster)
        {
            sum += new Vector4(color.R, color.G, color.B, color.A);
        }
        sum /= cluster.Count;
        return new Color(sum.X, sum.Y, sum.Z, sum.W);
    }

    private static float ColorDistance(Color c1, Color c2)
    {
        return Mathf.Sqrt(
            Mathf.Pow(c1.R - c2.R, 2) +
            Mathf.Pow(c1.G - c2.G, 2) +
            Mathf.Pow(c1.B - c2.B, 2)
        );
    }

    private static Godot.Collections.Array<Color> PadPalette(Godot.Collections.Array<Color> palette, int maxSize)
    {
        while (palette.Count < maxSize)
        {
            palette.Add(Colors.Black);
        }
        return palette;
    }



    // public async void GetOrUpdateTotalColorCount(Image image)
    // {
    //     if (image.IsEmpty())
    //     {
    //         NumColors = 0;
    //     }

    //     HashSet<ulong> uniqueColors = new HashSet<ulong>(); // Use ulong for color comparison

    //     for (int x = 0; x < image.GetWidth(); x++)
    //     {
    //         for (int y = 0; y < image.GetHeight(); y++)
    //         {
    //             Color color = image.GetPixel(x, y);
    //             // Convert Color to a single ulong for efficient comparison
    //             ulong colorValue = ((uint)color.R8 << 24) | ((uint)color.G8 << 16) | ((uint)color.B8 << 8) | (uint)color.A8;
    //             uniqueColors.Add(colorValue);
    //         }
    //     }
    //     NumColors = uniqueColors.Distinct().ToList().Count;
    // }

}