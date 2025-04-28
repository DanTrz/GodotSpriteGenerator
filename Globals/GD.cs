
using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

//Rename this class to "GD" to make it active or rename to anything elese to disable
public static class GD_Deactivated
{
    //private static readonly TaskFactory _taskFactory = new TaskFactory(TaskScheduler.Default);

    public static Variant BytesToVar(Span<byte> bytes) => Godot.GD.BytesToVar(bytes);
    public static Variant BytesToVarWithObjects(Span<byte> bytes) => Godot.GD.BytesToVarWithObjects(bytes);
    public static Variant Convert(Variant what, Variant.Type type) => Godot.GD.Convert(what, type);
    public static int Hash(Variant var) => Godot.GD.Hash(var);
    public static Resource Load(string path) => Godot.GD.Load(path);
    public static T Load<T>(string path) where T : class => Godot.GD.Load<T>(path);

    private static string AppendPrintParams(object[] parameters)
    {
        if (parameters == null || parameters.Length == 0)
            return "null";

        return string.Join(string.Empty, parameters.Select(p => p?.ToString() ?? "null"));
    }

    private static async Task PrintAsync(string what)
    {
        await Task.Yield(); // Yield to avoid potential starvation

        Godot.GD.Print(what);

        if (Debugger.IsAttached)
        {
            Debugger.Log(2, "inf", "Info: " + what + "\r\n");
        }
    }

    public static void Print(string what) => _ = PrintAsync(what);
    public static void Print(params object[] what) => Print(AppendPrintParams(what));

    public static void PrintRich(string what) => _ = PrintAsync(what);
    public static void PrintRich(params object[] what) => PrintRich(AppendPrintParams(what));

    public static void PrintErr(string what, string err, string error) => _ = PrintAsync($"{err}: {error}: {what}");
    public static void PrintErr(params object[] what) => PrintErr(AppendPrintParams(what), "err", "Error");

    public static void PrintRaw(string what) => _ = PrintAsync(what);
    public static void PrintRaw(params object[] what) => PrintRaw(AppendPrintParams(what));

    public static void PrintS(params object[] what)
    {
        Godot.GD.PrintS(what);
        _ = PrintAsync(AppendPrintParams(what) + " ");
    }

    public static void PrintT(params object[] what)
    {
        Godot.GD.PrintT(what);
        _ = PrintAsync(AppendPrintParams(what) + "\t");
    }

    public static void PushError(string message)
    {
        Godot.GD.PushError(message);
        if (Debugger.IsAttached) _ = PrintAsync($"Error: {message}");
    }
    public static void PushError(params object[] what) => PushError(AppendPrintParams(what));

    public static void PushWarning(string message)
    {
        Godot.GD.PushWarning(message);
        if (Debugger.IsAttached) _ = PrintAsync($"Warning: {message}");
    }
    public static void PushWarning(params object[] what) => PushWarning(AppendPrintParams(what));

    public static float Randf() => Godot.GD.Randf();
    public static double Randfn(double mean, double deviation) => Godot.GD.Randfn(mean, deviation);
    public static uint Randi() => Godot.GD.Randi();
    public static void Randomize() => Godot.GD.Randomize();
    public static double RandRange(double from, double to) => Godot.GD.RandRange(from, to);
    public static int RandRange(int from, int to) => Godot.GD.RandRange(from, to);
    public static uint RandFromSeed(ref ulong seed) => Godot.GD.RandFromSeed(ref seed);
    public static IEnumerable<int> Range(int end) => Godot.GD.Range(end);
    public static IEnumerable<int> Range(int start, int end) => Godot.GD.Range(start, end);
    public static IEnumerable<int> Range(int start, int end, int step) => Godot.GD.Range(start, end, step);
    public static void Seed(ulong seed) => Godot.GD.Seed(seed);
    public static Variant StrToVar(string str) => Godot.GD.StrToVar(str);
    public static byte[] VarToBytes(Variant var) => Godot.GD.VarToBytes(var);
    public static byte[] VarToBytesWithObjects(Variant var) => Godot.GD.VarToBytesWithObjects(var);
    public static string VarToStr(string var) => Godot.GD.VarToStr(var);
    public static Variant.Type TypeToVariantType(Type type) => Godot.GD.TypeToVariantType(type);
}