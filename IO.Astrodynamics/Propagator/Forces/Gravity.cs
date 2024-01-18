// Copyright 2024. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System.IO;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.OrbitalParameters;

namespace IO.Astrodynamics.Propagator.Forces;

public class Gravity : ForceBase
{
    private readonly GeopotentialModelReader _geopotentialModelReader;
    
    private double _gravitationalConstant = Constants.G; // G in m^3 kg^(-1) s^(-2)
    private double _earthMass; // Mass of the Earth in kg
    private double _earthRadius; // Mean radius of the Earth in meters

    public Gravity(FileInfo geopotentialModelFile)
    {
        _geopotentialModelReader = new GeopotentialModelReader(geopotentialModelFile);
    }


    public override Vector3 Apply(CelestialItem body, StateVector stateVector)
    {
        throw new System.NotImplementedException();
    }

    

    public Vector3 ComputeGravitationalForce(StateVector stateVector, ushort maxDegree=70, ushort maxOrder=70)
    {
        CelestialBody centerOfMotion = stateVector.Observer as CelestialBody;
        double r = stateVector.Position.Magnitude();
        var position = stateVector.Position;
        double u = position.Z / r;

        double forceX, forceY, forceZ;
        forceX = forceY = forceZ = 0.0;

        for (int n = 0; n <= maxDegree; n++)
        {
            for (int m = 0; m <= System.Math.Min(n, maxOrder); m++)
            {
                double Cnm = GetCoefficientC(n, m); // Replace with actual coefficient values
                double Snm = GetCoefficientS(n, m); // Replace with actual coefficient values

                double term1 = System.Math.Pow(centerOfMotion.EquatorialRadius / r, n);
                double term2 = Cnm * System.Math.Cos(m * System.Math.Atan2(position.Y, position.X)) + Snm * System.Math.Sin(m * System.Math.Atan2(position.Y, position.X));
                double term3 = AssociatedLegendrePolynomial(n, m, u);

                forceX += (centerOfMotion.GM / r) * term1 * term2 * term3 * (n + 1) * position.X / r;
                forceY += (centerOfMotion.GM / r) * term1 * term2 * term3 * (n + 1) * position.Y / r;
                forceZ += (centerOfMotion.GM / r) * term1 * term2 * term3 * (n + 1) * position.Z / r;
            }
        }

        return new Vector3(forceX, forceY, forceZ);
    }

    double AssociatedLegendrePolynomial(int n, int m, double u)
    {
        // Implement the associated Legendre polynomial calculation
        // This is a simplified version, and more accurate methods are available
        return System.Math.Pow(-1, m) * MathNet.Numerics.SpecialFunctions.(n, m, u);
    }

    // Replace these with the actual coefficient values from your geopotential model
    double GetCoefficientC(int n, int m)
    {
        // Implement coefficient retrieval logic
        return 0.0;
    }

    double GetCoefficientS(int n, int m)
    {
        // Implement coefficient retrieval logic
        return 0.0;
    }
}