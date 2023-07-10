using System.Text.Json.Serialization;
using IO.Astrodynamics.Math;

namespace IO.Astrodynamics.Coordinates
{
    public readonly record struct Planetodetic
    {
        public double Longitude { get; }
        public double Latitude { get; }
        public double Altitude { get; }

        public Planetodetic(double longitude, double latitude, double altitude)
        {
            Longitude = longitude;
            Latitude = latitude;
            Altitude = altitude;
        }

        public Planetocentric ToPlanetocentric(double flattening, double equatorialRadius)
        {
            double f2 = (1 - flattening) * (1 - flattening);
            double lat = System.Math.Atan(f2 * System.Math.Tan(Latitude));
            double a2 = equatorialRadius * equatorialRadius;
            double b = equatorialRadius - equatorialRadius * flattening;
            double b2 = b * b;
            double coslat2 = System.Math.Cos(Latitude) * System.Math.Cos(Latitude);
            double sinlat2 = System.Math.Sin(Latitude) * System.Math.Sin(Latitude);
            double alt = (a2 / System.Math.Sqrt(a2 * coslat2 + b2 * sinlat2)) * (1 / flattening) + Altitude;
            return new Planetocentric(Longitude, lat, alt);
        }

        // r = (a^2 / sqrt(a^2 * cos^2(lat) + b^2 * sin^2(lat))) + alt
    }
}