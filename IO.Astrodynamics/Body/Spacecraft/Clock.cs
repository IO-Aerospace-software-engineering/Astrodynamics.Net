using System;
using System.IO;
using System.Threading.Tasks;

namespace IO.Astrodynamics.Body.Spacecraft
{
    public class Clock : IEquatable<Clock>
    {
        public Spacecraft Spacecraft { get; private set; }
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

        internal void AttachSpacecraft(Spacecraft spacecraft)
        {
            Spacecraft = spacecraft;
        }

        public async Task WriteAsync(FileInfo outputFile)
        {
            await using var stream = this.GetType().Assembly.GetManifestResourceStream("IO.Astrodynamics.Templates.ClockTemplate.tsc");
            using StreamReader sr = new StreamReader(stream ?? throw new InvalidOperationException());
            var templateData = await sr.ReadToEndAsync();
            var data = templateData.Replace("{id}", Spacecraft.NaifId.ToString()).Replace("{resolution}", ((int)System.Math.Round(1.0 / Resolution)).ToString());
            await using var sw = new StreamWriter(outputFile.FullName);
            await sw.WriteAsync(data);
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