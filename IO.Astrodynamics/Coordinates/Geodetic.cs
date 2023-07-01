using System.Text.Json.Serialization;

namespace IO.Astrodynamics.Coordinates
{
    public readonly record struct Geodetic
    {
        public double Longitude { get; }
        public double Latitude { get; }
        public double Altitude { get; }

        [JsonConstructor]
        public Geodetic(double longitude, double latitude, double altitude)
        {
            Longitude = longitude;
            Latitude = latitude;
            Altitude = altitude;
        }
    }
}
