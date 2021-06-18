using System;

namespace Suruga.GlobalData
{
    public struct Paths
    {
        public static readonly string Base = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\Suruga";

        public static readonly string Configuration = $"{Base}\\Configuration.json";
    }
}
