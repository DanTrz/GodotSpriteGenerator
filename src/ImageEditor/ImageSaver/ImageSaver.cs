using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;

public partial class ImageSaver : Node
{
    [Export] private SubViewport _subViewport;
    [Export] public ImgColorReductionTextRect ImgColorReducTextRect;
    private readonly ConcurrentQueue<Dictionary<string, Image>> _imageQueue = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private readonly Task _processingTask;
    public static ImageSaver Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
    }

    //TODO: Add this in the correct place here => await _imgColorReductionTextRect.UpdateShaderParameters();



    public void AddImgToQueue(string savePath, Image image)
    {
        Dictionary<string, Image> imgData = new();
        imgData[savePath] = image;
        _imageQueue.Enqueue(imgData);

        GD.Print($"Image added to queue sucessfully: {Path.GetFileNameWithoutExtension(savePath)}");


    }

    public override void _Process(double delta)
    {
        if (_imageQueue.Count > 0)
        {
            Task.Run(() => ProcessQueue());
        }
    }


    private async Task ProcessQueue()
    {
        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            if (_imageQueue.TryDequeue(out Dictionary<string, Image> imgData))
            {
                string savePath = imgData.Keys.First();
                GD.Print($"Queue Processing => Getting image from queue: {Path.GetFileNameWithoutExtension(savePath)}");

                //TODO: Update this Logic for ImageSaaver
                // From here Call a new method that will 
                // 1. Apply the Transform Effect and reduce color image
                // 2. Wait for the Transform effect to finish via a signal
                // 4. Save the PNG them one by one listening to the effect signal finished.

                await Task.Run(() => SaveAsPng(savePath, imgData.Values.First())); // Run each save in parallel
            }
            // else
            // {
            //     await Task.Delay(100); // Small delay when queue is empty
            // }
        }
    }



    private void SaveAsPng(string savePath, Image img)
    {
        GD.Print($"Queue Processing => Start to save image: {Path.GetFileNameWithoutExtension(savePath)}");
        try
        {
            string path = $"{savePath}.png";
            img.SavePng(ProjectSettings.GlobalizePath(path));
            GD.Print($"Queue Processing => Image saved as PNG: {Path.GetFileNameWithoutExtension(savePath)}");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Queue Processing => Error processing image {savePath}: {ex.Message}");
        }





        // try
        // {
        //     Image image = new Image();
        //     byte[] fileData = File.ReadAllBytes(imagePath);
        //     image.LoadPngFromBuffer(fileData);

        //     string pngPath = Path.ChangeExtension(imagePath, ".png");
        //     image.SavePng(pngPath);

        //     GD.Print($"Image saved as PNG: {pngPath}");
        // }
        // catch (Exception ex)
        // {
        //     GD.PrintErr($"Error processing image {imagePath}: {ex.Message}");
        // }
    }

    public void StopProcessing()
    {
        _cancellationTokenSource.Cancel();
        _processingTask.Wait(); // Wait for the processing task to finish
        GD.Print("Queue Processing => Image saver stopped.");
    }
}

