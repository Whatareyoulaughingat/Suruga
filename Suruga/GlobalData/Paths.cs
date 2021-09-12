namespace Suruga.GlobalData;

public static class Paths
{
    public static string Base => Path.GetFullPath("Suruga", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));

    public static string Configuration => Path.GetFullPath("Configuration.json", Base);
}
