using IO.Astrodynamics.Models.Body;
using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models.Mission;
using IO.Astrodynamics.Models.OrbitalParameters;
using System;
using IO.Astrodynamics.Models.Time;

namespace IO.Astrodynamics.Models.Tests
{
    internal static class TestHelpers
    {
        internal static CelestialBody GetSun()
        {
            return new CelestialBody(10, "sun", 1.32712440018E+11, 695508.0, 695508.0);
        }

        internal static CelestialBody GetEarth()
        {
            var earthScn = new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366, new Frames.Frame("ITRF93"),
                new StateVector(new Vector3(-2.679537555216521E+07, 1.327011135216045E+08, 5.752533467064925E+07),
                    new Vector3(-2.976558008982104E+01, -5.075339952746913E+00, -2.200929976753953E+00), GetSun(), new DateTime(2021, 1, 1), Frames.Frame.ICRF));
            return earthScn;
        }


        internal static CelestialBody GetMoon()
        {
            return new CelestialBody(301, "moon", 4.902E+3, 1736.0, 1738.1, new Frames.Frame("IAU_MOON"),
                new StateVector(new Vector3(-2.068864826237993E+05, 2.891146390982051E+05, 1.515746884380044E+05),
                    new Vector3(-8.366764389833921E-01, -5.602543663174073E-01, -1.710459390585548E-01), GetEarth(), new DateTime(2021, 1, 1), Frames.Frame.ICRF));
        }

        internal static CelestialBody GetMars()
        {
            return new CelestialBody(499, "mars", 4.2828E+4, 3376.2, 3396.2, new Frames.Frame("IAU_MARS"),
                new StateVector(new Vector3(92881635.80779284, 188006710.59934112, 83728055.58077827), new Vector3(-21.16658268, 10.72779167, 5.49171534), GetSun(),
                    new DateTime(2021, 1, 1), Frames.Frame.ICRF));
        }

        internal static CelestialBody GetEarthAtJ2000()
        {
            return new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366, new Frames.Frame("ITRF93"),
                new StateVector(new Vector3(-26499033.67742509, 132757417.33833946, 57556718.47053819), new Vector3(-29.79426007, -5.01805231, -2.17539380), GetSun(),
                    new DateTime(2000, 1, 1, 12, 0, 0), Frames.Frame.ICRF));
        }

        internal static CelestialBody GetEarthAtJ20011214()
        {
            return new CelestialBody(399, "earth", 3.986004418E+5, 6356.7519, 6378.1366, new Frames.Frame("ITRF93"),
                new StateVector(new Vector3(20406240.51495151, 133809670.48912178, 58012572.06513986), new Vector3(-29.99780186, 3.68842732, 1.60029151), GetSun(),
                    new DateTime(2001, 12, 14, 0, 0, 0), Frames.Frame.ICRF));
        }

        internal static CelestialBody GetMoonAtJ2000()
        {
            return new CelestialBody(301, "moon", 4.902E+3, 1736.0, 1738.1, new Frames.Frame("IAU_MOON"),
                new StateVector(new Vector3(-291608.38463344, -266716.83339423, -76102.48709990), new Vector3(0.64353139, -0.66608768, -0.30132570), GetEarthAtJ2000(),
                    new DateTime(2000, 1, 1, 12, 0, 0), Frames.Frame.ICRF));
        }

        internal static CelestialBody GetMoonAt20011214()
        {
            return new CelestialBody(301, "moon", 4.902E+3, 1736.0, 1738.1, new Frames.Frame("IAU_MOON"),
                new StateVector(new Vector3(-121546.11992047, -336069.18056094, -135303.93452846), new Vector3(0.96178812, -0.30184678, -0.22950150), GetEarthAtJ20011214(),
                    new DateTime(2001, 12, 14, 0, 0, 0), Frames.Frame.ICRF));
        }

        internal static CelestialBody GetMarsAtJ2000()
        {
            return new CelestialBody(499, "mars", 4.2828E+4, 3376.2, 3396.2, new Frames.Frame("IAU_MARS"),
                new StateVector(new Vector3(208048140.64184200, 209619.17353881, -5529162.31315533), new Vector3(1.16267240, 23.91840970, 10.93917192), GetSun(),
                    new DateTime(2000, 1, 1, 12, 0, 0), Frames.Frame.ICRF));
        }
    }
}