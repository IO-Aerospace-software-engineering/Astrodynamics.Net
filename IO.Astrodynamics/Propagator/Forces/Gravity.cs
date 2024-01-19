// Copyright 2024. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System.IO;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.OrbitalParameters;

namespace IO.Astrodynamics.Propagator.Forces;

public class Gravity : ForceBase
{
    private readonly GeopotentialModelReader _geopotentialModelReader;

    public Gravity(FileInfo geopotentialModelFile)
    {
        _geopotentialModelReader = new GeopotentialModelReader(geopotentialModelFile);
    }

    public override Vector3 Apply(CelestialItem body, StateVector stateVector)
    {
        return ComputeGravitationalForce(stateVector);
    }

    public Vector3 ComputeGravitationalForce(StateVector stateVector, ushort maxDegree = 70)
    {
        CelestialBody centerOfMotion = stateVector.Observer as CelestialBody;
        double r = stateVector.Position.Magnitude();
        var position = stateVector.Position;
        double u = position.Z / r;

        double forceX, forceY, forceZ;
        double gmr = centerOfMotion.GM / r;
        double eqr = centerOfMotion.EquatorialRadius / r;
        double omegaN = 0.0;
        double eqrn = 0.0;

        for (ushort n = 0; n <= maxDegree; n++)
        {
            eqrn += System.Math.Pow(eqr, n);
            
            for (ushort m = 0; m <= n; m++)
            {
                double Cnm = GetCoefficientC(n, m);
                double Snm = GetCoefficientS(n, m);

                omegaN += LegendreFunctions.NormalizedAssociatedLegendre(n, m, u) * (Cnm * System.Math.Cos(m * System.Math.Atan2(position.Y, position.X)) +
                                                                                     Snm * System.Math.Sin(m * System.Math.Atan2(position.Y, position.X)));

                // double term1 = System.Math.Pow(centerOfMotion.EquatorialRadius / r, n);
                // double term2 = Cnm * System.Math.Cos(m * System.Math.Atan2(position.Y, position.X)) + Snm * System.Math.Sin(m * System.Math.Atan2(position.Y, position.X));
                // double term3 = LegendreFunctions.NormalizedAssociatedLegendre(n, m, u);
                // double term4 = term1 * term2 * term3 * (n + 1);
                // double term5 = centerOfMotion.GM / r;
                //
                // forceX += term5 * term4 * position.X / r;
                // forceY += term5 * term4 * position.Y / r;
                // forceZ += term5 * term4 * position.Z / r;
            }
        }

        var res = gmr * (1 + eqrn * omegaN);
        forceX = res * position.X / r;
        forceY = res * position.Y / r;
        forceZ = res * position.Z / r;
        return new Vector3(forceX, forceY, forceZ);
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