using System.Numerics;
using IO.Astrodynamics.Body;
using Vector3 = IO.Astrodynamics.Math.Vector3;

namespace IO.Astrodynamics.Coordinates
{
    public readonly record struct Planetocentric
    {
        public double Longitude { get; }
        public double Latitude { get; }
        public double Radius { get; }

        public Planetocentric(double longitude, double latitude, double radius)
        {
            Longitude = longitude;
            Latitude = latitude;
            Radius = radius;
        }

        public Planetodetic ToPlanetodetic(double flattening, double equatorialRadius)
        {
            double f2 = (1 - flattening) * (1 - flattening);
            double lat = System.Math.Atan((1.0 / f2) * System.Math.Tan(Latitude));
            double alt = 0.0;
            return new Planetodetic(Longitude, lat, alt);
        }

        public double RadiusFromPlanetocentricLatitude(double equatorialRadius, double flattening)
        {
            return RadiusFromPlanetocentricLatitude(Latitude, equatorialRadius, flattening);
        }

        public static double RadiusFromPlanetocentricLatitude(double latitude, double equatorialRadius, double flattening)
        {
            double r2 = equatorialRadius * equatorialRadius;
            double s2 = System.Math.Sin(latitude) * System.Math.Sin(latitude);
            double f2 = (1 - flattening) * (1 - flattening);
            return System.Math.Sqrt(r2 / (1 + (1 / f2 - 1) * s2));
        }

        public Vector3 ToCartesianCoordinates()
        {
            var x = Radius * System.Math.Cos(Longitude) * System.Math.Cos(Latitude);
            var y = Radius * System.Math.Sin(Longitude)* System.Math.Cos(Latitude);
            var z = Radius * System.Math.Sin(Latitude);
            return new Vector3(x, y, z);
        }
    }
}