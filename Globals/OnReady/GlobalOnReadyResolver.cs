using Godot;

/// The resolver is not working on the main scene of the project @see https://github.com/godotengine/godot/issues/37813,
/// so we need to use a decorator Root scene to instantiate the real main scene of the game, which is covered by the injector
public partial class GlobalOnReadyResolver : Node
{
    public override void _Ready()
    {
        // Listen for new nodes added to the scene tree
        GetTree().NodeAdded += OnNodeAdded;

        // Resolver needs to be executed for all nodes already in the scene tree (auto-load nodes)
        foreach (var node in GetTree().Root.GetChildren())
        {
            OnNodeAdded(node);
        }
    }

    private void OnNodeAdded(Node node)
    {
        //// // Resolve internal node references by resolving [OnReady] attributes

        //var name = node.Name.ToString();
        OnReadyResolver.Resolve(node);
    }

}