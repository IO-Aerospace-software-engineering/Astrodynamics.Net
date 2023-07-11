using System;

namespace IO.Astrodynamics.Body.Spacecraft
{
    public class Clock : IEquatable<Clock>
    {
        public string Name { get; }
        public double Resolution { get; }

        public Clock(string name, double resolution)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Clock requires a name");
            }

            if (resolution <= 0.0)
            {
                throw new ArgumentException("Resolution must be a positive number");
            }

            Name = name;
            Resolution = resolution;
        }

        public bool Equals(Clock other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase) && System.Math.Abs(Resolution - other.Resolution) < double.Epsilon;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Clock)obj);
        }

        public override int GetHashCode()
        {
            return (Name != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(Name) : 0);
        }

        public static bool operator ==(Clock left, Clock right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Clock left, Clock right)
        {
            return !Equals(left, right);
        }
    }
}