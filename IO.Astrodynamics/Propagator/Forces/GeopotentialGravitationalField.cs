// Copyright 2024. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System.IO;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.Coordinates;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.OrbitalParameters;

namespace IO.Astrodynamics.Propagator.Forces;

public class GeopotentialGravitationalField : GravitationalField
{
    private readonly GeopotentialModelReader _geopotentialModelReader;

    public GeopotentialGravitationalField(FileInfo geopotentialModelFile)
    {
        _geopotentialModelReader = new GeopotentialModelReader(geopotentialModelFile);
    }

    public override Vector3 Apply(StateVector stateVector)
    {
        return ComputeGravitationalAcceleration(stateVector);
    }

    public Vector3 ComputeGravitationalAcceleration(StateVector stateVector, ushort maxDegree = 70)
    {
        CelestialBody centerOfMotion = stateVector.Observer as CelestialBody;

        double r = stateVector.Position.Magnitude();
        var position = stateVector.Position;
        double theta = System.Math.Abs(System.Math.Asin(position.Z / r) - Constants.PI2);

        double gmr = -centerOfMotion.GM / r;
        double eqr = centerOfMotion.EquatorialRadius / r;
        double omegaN = 0.0;
        double eqrn = 0.0;

        for (ushort n = 2; n <= maxDegree; n++)
        {
            eqrn = System.Math.Pow(eqr, n);

            for (ushort m = 0; m <= n; m++)
            {
                double Cnm = GetCoefficientC(n, m);
                double Snm = GetCoefficientS(n, m);
                double longitude_m = m * System.Math.Atan2(position.Y, position.X);

                omegaN += eqrn * LegendreFunctions.NormalizedAssociatedLegendre(n, m, theta) * (Cnm * System.Math.Cos(longitude_m));
                if (m != 0)
                {
                    omegaN += eqrn * LegendreFunctions.NormalizedAssociatedLegendre(n, m, theta) * (Snm * System.Math.Sin(longitude_m));
                }
            }
        }

        var res = gmr * (1 + omegaN) / r;
        
        var aX = res * position.X / r;
        var aY = res * position.Y / r;
        var aZ = res * position.Z / r;
        return new Vector3(aX, aY, aZ);
    }

    double GetCoefficientC(ushort n, ushort m)
    {
        return _geopotentialModelReader.ReadCoefficient(n, m).C;
    }

    double GetCoefficientS(ushort n, ushort m)
    {
        return _geopotentialModelReader.ReadCoefficient(n, m).S;
    }
}