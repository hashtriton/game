// Entry points for `unity command run_script` (run in the live Editor; the file stays outside Assets).
using System.IO;
using System.Linq;

public static class UnityImportEntry
{
    const string Out = "../verification/creatures/";

    public static string ImportAll()
    {
        UnityEditor.AssetDatabase.Refresh();
        var names = CreatureImporter.ImportAll();
        return string.Join(",", names) + " | " + CreatureImporter.BuildShowcase();
    }

    public static string CaptureOne(string name)
    {
        string dir = Path.GetFullPath(Out);
        return string.Join(",", new[] { ("Idle", 0f), ("Attack", .45f), ("Death", 1f) }.Select(c =>
            CreatureImporter.Capture(c.Item1, c.Item2, dir + "unity-" + name + "-" + c.Item1.ToLowerInvariant() + ".png", 20, 28, name)));
    }

    public static string CaptureShowcase()
    {
        string dir = Path.GetFullPath(Out);
        return CreatureImporter.Capture("Idle", 0f, dir + "unity-showcase-idle.png", 0, 38) + "," +
               CreatureImporter.Capture("Attack", .45f, dir + "unity-showcase-attack.png", 0, 38);
    }
}
