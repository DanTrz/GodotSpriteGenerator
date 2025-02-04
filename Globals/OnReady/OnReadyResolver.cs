using System;
using Godot;

public static class OnReadyResolver
{
    public static void Resolve(Node target)
    {
        var fields = target.GetType().GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        foreach (var field in fields)
        {
            if (Attribute.GetCustomAttribute(field, typeof(OnReadyAttribute)) is OnReadyAttribute attribute)
            {
                var nodePath = attribute.NodePath;
                var nodeType = field.FieldType;

                // Use GetNode to retrieve the node
                var node = target.GetNode(nodePath);

                // Assign the retrieved node to the corresponding field
                field.SetValue(target, node);
            }
        }
    }
}