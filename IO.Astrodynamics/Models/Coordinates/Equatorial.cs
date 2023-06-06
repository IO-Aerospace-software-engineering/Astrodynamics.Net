﻿using IO.Astrodynamics.Models.OrbitalParameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IO.Astrodynamics.Models.Coordinates
{
    public readonly record struct Equatorial
    {
        public Equatorial(double declination, double rightAscencion, double distance)
        {
            Declination = declination;
            RightAscencion = rightAscencion;
            Distance = distance;
        }

        public Equatorial(StateVector stateVector)
        {
            var sv = stateVector.ToFrame(Frames.Frame.ICRF).ToStateVector();

            Distance = sv.Position.Magnitude();
            RightAscencion = System.Math.Atan2(sv.Position.Y , sv.Position.X);
            if (RightAscencion < 0)
                RightAscencion += Constants._2PI;

            Declination = System.Math.Asin(sv.Position.Z / Distance);
        }

        public double Declination { get; }
        public double RightAscencion { get; }
        public double Distance { get; }
    }
}