using System;
using IO.Astrodynamics.Frames;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.OrbitalParameters;

namespace IO.Astrodynamics.Coordinates
{
    public readonly record struct Equatorial
    {
        public double Declination { get; }
        public double RightAscension { get; }
        public double Distance { get; }

        public Equatorial(double declination, double rightAscension, double distance)
        {
            Declination = declination;
            RightAscension = rightAscension;
            Distance = distance;
        }

        public Equatorial(double declination, double rightAscension) : this(declination, rightAscension, Double.NaN)
        {
        }

        public Equatorial(StateVector stateVector)
        {
            var sv = stateVector.ToFrame(Frame.ICRF).ToStateVector();

            Distance = sv.Position.Magnitude();
            RightAscension = System.Math.Atan2(sv.Position.Y, sv.Position.X);
            if (RightAscension < 0)
                RightAscension += Constants._2PI;

            Declination = System.Math.Asin(sv.Position.Z / Distance);
        }

        /// <summary>
        /// Convert to cartesian coordinates
        /// </summary>
        /// <returns></returns>
        public Vector3 ToCartesian()
        {
            double x = Distance * System.Math.Cos(Declination) * System.Math.Cos(RightAscension);
            double y = Distance * System.Math.Cos(Declination) * System.Math.Sin(RightAscension);
            double z = Distance * System.Math.Sin(Declination);
            return new Vector3(x, y, z);
        }
    }
}