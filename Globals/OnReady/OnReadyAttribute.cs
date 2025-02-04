using System;

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class OnReadyAttribute : Attribute
{
    public string NodePath { get; }

    public OnReadyAttribute(string nodePath)
    {
        NodePath = nodePath;
    }
}