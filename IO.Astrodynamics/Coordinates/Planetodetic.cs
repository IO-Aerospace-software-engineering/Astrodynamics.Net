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
            return new Planetocentric(Longitude, lat, Planetocentric.RadiusFromPlanetocentricLatitude(lat, equatorialRadius, flattening) + Altitude);
        }
        
        
    }
}