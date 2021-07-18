using System;

namespace Suruga.GlobalData
{
    public struct Paths : IEquatable<Paths>
    {
        public readonly string Base
        {
            get => $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\Suruga";
        }

        public readonly string Configuration
        {
            get => $"{Base}\\Configuration.yml";
        }

        public static bool operator ==(Paths left, Paths right)
            => Equals(left, right);

        public static bool operator !=(Paths left, Paths right)
            => !Equals(left, right);

        public bool Equals(Paths other)
            => (Base, Configuration) == (other.Base, other.Configuration);

        public override bool Equals(object obj)
            => (obj is Paths paths) && Equals(paths);

        public override int GetHashCode()
            => (Base, Configuration).GetHashCode();
    }
}
