using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace IO.Astrodynamics.Models.Coordinates
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
