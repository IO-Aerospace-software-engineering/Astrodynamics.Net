using IO.Astrodynamics.Models;
using IO.Astrodynamics.Models.OrbitalParameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IO.Astrodynamics.Models.Coordinates
{
    public readonly record struct Horizontal
    {
        public double Azimuth { get; }
        public double Elevation { get; }
        public double Altitude { get; }


        public Horizontal(double azimuth, double elevation, double altitude)
        {
            Azimuth = azimuth;
            Elevation = elevation;
            Altitude = altitude;
        }
    }
}
