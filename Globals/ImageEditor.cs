using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

[Tool]
public partial class ImageEditor : PanelContainer
{
    [ExportToolButton("UpdateShader")] public Callable ClickMeButton => Callable.From(UpdateShaderParameters);
    [Export] public TextureRect ImgTextRect;
    [Export] public bool EnableColorReduction = false;
    [Export] public int NumColors = 16;
    [Export] public bool UseExternalPalette = false;
    [Export] public Godot.Collections.Array<Color> ExternalPalette = new();
    [Export] public bool EnableSaturation = true;
    [Export] public float Saturation = 1.0f;
    [Export] public bool EnableBrightness = true;
    [Export] public float Brightness = 1.0f;



    private const int MaxPaletteSize = 256;

    public void UpdateShaderParameters()
    {
        Texture currentTexture = ImgTextRect.Texture;

        if (ImgTextRect.Material is not ShaderMaterial shaderMaterial)
        {
            GD.PrintErr("Material is not a ShaderMaterial.");
            return;
        }

        shaderMaterial.SetShaderParameter("enable_saturation", EnableSaturation);
        shaderMaterial.SetShaderParameter("saturation", Saturation);
        shaderMaterial.SetShaderParameter("enable_brightness", EnableBrightness);
        shaderMaterial.SetShaderParameter("brightness", Brightness);

        shaderMaterial.SetShaderParameter("enable_color_reduction", EnableColorReduction);
        shaderMaterial.SetShaderParameter("num_colors", NumColors);
        shaderMaterial.SetShaderParameter("use_external_palette", UseExternalPalette);

        if (!EnableColorReduction) return;

        Godot.Collections.Array<Color> palette = new();

        if (UseExternalPalette)
        {
            palette = PadPalette(ExternalPalette, MaxPaletteSize);
        }
        else
        {
            if (currentTexture is Texture2D texture2D)
            {
                Image image = texture2D.GetImage();
                if (image.GetSize() == Vector2.Zero)
                {
                    GD.PrintErr("Texture has zero size, dynamic palette generation will not work.");
                    for (int i = 0; i < MaxPaletteSize; i++)
                    {
                        palette.Add(Colors.Black);
                    }
                }
                else
                {
                    List<Color> kMeansPalette = KMeansClustering(image, NumColors);
                    palette = PadPalette(new Godot.Collections.Array<Color>(kMeansPalette), MaxPaletteSize);
                }
            }
            else
            {
                GD.PrintErr("Texture is null or not a Texture2D, dynamic palette generation will not work.");
                for (int i = 0; i < MaxPaletteSize; i++)
                {
                    palette.Add(Colors.Black);
                }
            }
        }

        shaderMaterial.SetShaderParameter("external_palette", palette);
    }

    private static List<Color> KMeansClustering(Image image, int k)
    {
        if (image == null || image.GetWidth() == 0 || image.GetHeight() == 0)
        {
            return Enumerable.Repeat(Colors.Black, k).ToList();
        }

        // 1. Get *all* colors (not just unique) and their frequencies.
        Dictionary<Color, int> colorFrequencies = new();
        for (int y = 0; y < image.GetHeight(); y++)
        {
            for (int x = 0; x < image.GetWidth(); x++)
            {
                Color pixelColor = image.GetPixel(x, y);
                if (pixelColor.A > 0) // Consider alpha
                {
                    if (colorFrequencies.ContainsKey(pixelColor))
                    {
                        colorFrequencies[pixelColor]++;
                    }
                    else
                    {
                        colorFrequencies[pixelColor] = 1;
                    }
                }
            }
        }


        // 2. Handle cases where the number of unique colors is less than k.
        if (colorFrequencies.Count <= k)
        {
            List<Color> result = colorFrequencies.Keys.ToList();
            while (result.Count < k)
            {
                result.Add(Colors.Black); // Pad with black
            }
            return result;
        }


        // 3.  Initialization:  A hybrid approach.
        List<Color> centroids = new();
        // 3a.  Prioritize the MOST FREQUENT colors.
        List<Color> sortedColors = colorFrequencies.OrderByDescending(pair => pair.Value).Select(pair => pair.Key).ToList();
        centroids.AddRange(sortedColors.Take(Math.Min(k, sortedColors.Count))); // Add the top 'k' (or fewer) most frequent

        // 3b. If we still need more centroids (unlikely, but possible), fill the rest randomly.
        if (centroids.Count < k)
        {
            HashSet<Color> uniqueColors = new HashSet<Color>(colorFrequencies.Keys);
            centroids.AddRange(uniqueColors.OrderBy(_ => GD.Randi()).Take(k - centroids.Count));
        }

        // 4.  Iteration (rest of the K-Means algorithm remains the same).
        int maxIterations = 100;
        List<Color>[] clusters = new List<Color>[k];
        for (int i = 0; i < k; i++) clusters[i] = new();

        for (int iteration = 0; iteration < maxIterations; iteration++)
        {
            for (int i = 0; i < k; i++) clusters[i].Clear();

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

            for (int i = 0; i < k; i++)
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
        return centroids;
    }

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
}