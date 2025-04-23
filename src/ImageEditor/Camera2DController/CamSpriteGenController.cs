using System;
using Godot;

public partial class CamSpriteGenController : Camera2D
{
    [Export] public float ZoomSpeed = 0.01f;
    [Export] public Vector2 MinZoom = new Vector2(0.1f, 0.1f);
    [Export] public Vector2 MaxZoom = new Vector2(10.0f, 10.0f);
    [Export] public Vector2 DefaultZoom = new Vector2(1.0f, 1.0f);

    [Export] public SubViewportContainer ParentSubViewportContainer;

    private Vector2 _defaultPosition;
    private bool _isPanning = false;
    private Vector2 _previousMousePosition;

    public override void _Ready()
    {
        _defaultPosition = Position;
        MakeCurrent();
        Zoom = DefaultZoom;
        GlobalEvents.Instance.OnCamResetChanges += ResetCamera;
    }

    public void PanViewToMousePos(InputEventMouseMotion motionEvent)
    {
        Position -= motionEvent.Relative / Zoom;
    }

    public void ZoomTowardsMouse(bool zoomIn)
    {
        //bool True = Zoom In, False = Zoom Out
        var zoomDir = (!zoomIn) ? -1 : 1;

        Vector2 newZoom = Zoom + new Vector2(ZoomSpeed, ZoomSpeed) * zoomDir;
        newZoom = newZoom.Clamp(MinZoom, MaxZoom);

        if (newZoom != Zoom)
        {

            // Get the mouse position in the SubViewport
            Vector2 mousePosInSubViewport = ParentSubViewportContainer.GetLocalMousePosition();

            // Calculate world position under the mouse before zoom
            Vector2 preZoomWorldPos = GetScreenTransform().AffineInverse().BasisXform(mousePosInSubViewport);

            Zoom = newZoom;

            // Calculate world position under the mouse after zoom
            Vector2 postZoomWorldPos = GetScreenTransform().AffineInverse().BasisXform(mousePosInSubViewport);

            Position += preZoomWorldPos - postZoomWorldPos;

            Zoom = newZoom;
        }


        // Callable.From(() => BgGrid.QueueRedraw()).CallDeferred();


    }


    public void ResetCamera()
    {
        Position = _defaultPosition;
        Zoom = DefaultZoom;
    }
}
