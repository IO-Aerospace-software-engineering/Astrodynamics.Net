using System;
using System.Collections.Generic;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.Coordinates;
using IO.Astrodynamics.Frames;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.Time;

namespace IO.Astrodynamics.OrbitalParameters;

public abstract class OrbitalParameters : IEquatable<OrbitalParameters>
{
    public CelestialBody CenterOfMotion { get; protected set; }

    public DateTime Epoch { get; }

    public Frame Frame { get; }

    protected OrbitalParameters()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="centerOfMotion"></param>
    /// <param name="epoch"></param>
    /// <param name="frame"></param>
    protected OrbitalParameters(CelestialBody centerOfMotion, DateTime epoch, Frame frame)
    {
        CenterOfMotion = centerOfMotion;
        Epoch = epoch;
        Frame = frame;
    }

    /// <summary>
    /// Get eccentric vector
    /// </summary>
    /// <returns></returns>
    public virtual Vector3 EccentricityVector()
    {
        return ToStateVector().EccentricityVector();
    }

    /// <summary>
    /// Get eccentricity
    /// </summary>
    /// <returns></returns>
    public abstract double Eccentricity();

    /// <summary>
    /// Get the specific angular momentum
    /// </summary>
    /// <returns></returns>
    public virtual Vector3 SpecificAngularMomentum()
    {
        return ToStateVector().SpecificAngularMomentum();
    }

    /// <summary>
    /// Get the specific orbital energy in MJ
    /// </summary>
    /// <returns></returns>
    public virtual double SpecificOrbitalEnergy()
    {
        return ToStateVector().SpecificOrbitalEnergy();
    }

    /// <summary>
    /// Get inclination
    /// </summary>
    /// <returns></returns>
    public abstract double Inclination();

    /// <summary>
    /// Get the semi major axis
    /// </summary>
    /// <returns></returns>
    public abstract double SemiMajorAxis();

    /// <summary>
    /// Get vector to ascending node unitless
    /// </summary>
    /// <returns></returns>
    public virtual Vector3 AscendingNodeVector()
    {
        if (Inclination() == 0.0)
        {
            return Vector3.VectorX;
        }

        return ToStateVector().AscendingNodeVector();
    }

    /// <summary>
    /// Get vector to descending node unitless
    /// </summary>
    /// <returns></returns>
    public virtual Vector3 DescendingNodeVector()
    {
        return AscendingNodeVector().Inverse();
    }

    /// <summary>
    /// Get ascending node angle
    /// </summary>
    /// <returns></returns>
    public abstract double AscendingNode();

    /// <summary>
    /// Get the argument of periapis
    /// </summary>
    /// <returns></returns>
    public abstract double ArgumentOfPeriapsis();

    /// <summary>
    /// Get the true anomaly
    /// </summary>
    /// <returns></returns>
    public abstract double TrueAnomaly();

    /// <summary>
    /// Get the eccentric anomaly
    /// </summary>
    /// <returns></returns>
    public abstract double EccentricAnomaly();

    /// <summary>
    /// Get the mean anomaly
    /// </summary>
    /// <returns></returns>
    public abstract double MeanAnomaly();

    /// <summary>
    /// Compute mean anomaly from true anomaly
    /// </summary>
    /// <param name="trueAnomaly"></param>
    /// <returns></returns>
    public double MeanAnomaly(double trueAnomaly)
    {
        if (trueAnomaly < 0.0)
        {
            trueAnomaly += Constants._2PI;
        }

        //X = cos E
        double x = (Eccentricity() + System.Math.Cos(trueAnomaly)) / (1 + Eccentricity() * System.Math.Cos(trueAnomaly));
        double eccAno = System.Math.Acos(x);
        double M = eccAno - Eccentricity() * System.Math.Sin(eccAno);

        if (trueAnomaly > Constants.PI)
        {
            M = Constants._2PI - M;
        }

        return M;
    }

    /// <summary>
    /// Get orbital period
    /// </summary>
    /// <returns></returns>
    public TimeSpan Period()
    {
        double a = SemiMajorAxis();
        double T = Constants._2PI * System.Math.Sqrt((a * a * a) / CenterOfMotion.GM);
        return TimeSpan.FromSeconds(T);
    }

    /// <summary>
    /// Get orbital mean motion
    /// </summary>
    /// <returns></returns>
    public double MeanMotion()
    {
        return Constants._2PI / Period().TotalSeconds;
    }


    /// <summary>
    /// Get the state vector
    /// </summary>
    /// <returns></returns>
    public virtual StateVector ToStateVector()
    {
        var e = Eccentricity();
        var p = SemiMajorAxis() * (1 - e * e);
        var v = TrueAnomaly();
        var r0 = p / (1 + e * System.Math.Cos(v));
        var x = r0 * System.Math.Cos(v);
        var y = r0 * System.Math.Sin(v);
        var dotX = -System.Math.Sqrt(CenterOfMotion.GM / p) * System.Math.Sin(v);
        var dotY = System.Math.Sqrt(CenterOfMotion.GM / p) * (e + System.Math.Cos(v));
        Matrix R3 = Matrix.CreateRotationMatrixZ(-AscendingNode());
        Matrix R1 = Matrix.CreateRotationMatrixX(-Inclination());
        Matrix R3w = Matrix.CreateRotationMatrixZ(-ArgumentOfPeriapsis());
        Matrix R = R3 * R1 * R3w;
        double[] pos = { x, y, 0.0 };
        double[] vel = { dotX, dotY, 0.0 };
        double[] finalPos = pos * R;
        double[] finalV = vel * R;

        return new StateVector(new Vector3(finalPos[0], finalPos[1], finalPos[2]), new Vector3(finalV[0], finalV[1], finalV[2]), CenterOfMotion, Epoch, Frame);
    }

    /// <summary>
    /// Convert to equinoctial
    /// </summary>
    /// <returns></returns>
    public virtual EquinoctialElements ToEquinoctial()
    {
        double e = Eccentricity();
        double o = AscendingNode();
        double w = ArgumentOfPeriapsis();
        double i = Inclination();
        double v = TrueAnomaly();

        double p = SemiMajorAxis() * (1 - e * e);
        double f = e * System.Math.Cos(o + w);
        double g = e * System.Math.Sin(o + w);
        double h = System.Math.Tan(i * 0.5) * System.Math.Cos(o);
        double k = System.Math.Tan(i * 0.5) * System.Math.Sin(o);
        double l0 = o + w + v;

        return new EquinoctialElements(p, f, g, h, k, l0, CenterOfMotion, Epoch, Frame);
    }

    /// <summary>
    /// Get perigee vector
    /// </summary>
    /// <returns></returns>
    public virtual Vector3 PerigeeVector()
    {
        if (Eccentricity() == 0.0)
        {
            return Vector3.VectorX * SemiMajorAxis();
        }

        return EccentricityVector().Normalize() * SemiMajorAxis() * (1.0 - Eccentricity());
    }

    /// <summary>
    /// Get apogee vector
    /// </summary>
    /// <returns></returns>
    public virtual Vector3 ApogeeVector()
    {
        if (Eccentricity() == 0.0)
        {
            return Vector3.VectorX.Inverse() * SemiMajorAxis();
        }

        return EccentricityVector().Normalize().Inverse() * SemiMajorAxis() * (1.0 + Eccentricity());
    }

    /// <summary>
    /// Get orbital parameters at epoch
    /// </summary>
    /// <param name="epoch"></param>
    /// <returns></returns>
    public OrbitalParameters AtEpoch(DateTime epoch)
    {
        return ToKeplerianElements(epoch);
    }

    /// <summary>
    /// Get the true longitude
    /// </summary>
    /// <returns></returns>
    public double TrueLongitude()
    {
        return (AscendingNode() + ArgumentOfPeriapsis() + TrueAnomaly()) % Constants._2PI;
    }

    /// <summary>
    /// Get the mean longitude
    /// </summary>
    /// <returns></returns>
    public double MeanLongitude()
    {
        return (AscendingNode() + ArgumentOfPeriapsis() + MeanAnomaly()) % Constants._2PI;
    }

    public bool IsCircular()
    {
        return Eccentricity() == 0.0;
    }

    public bool IsParabolic()
    {
        return Eccentricity() == 1.0;
    }

    public bool IsHyperbolic()
    {
        return Eccentricity() > 1.0;
    }

    private KeplerianElements ToKeplerianElements(DateTime epoch)
    {
        double ellapsedTime = epoch.SecondsFromJ2000TDB() - Epoch.SecondsFromJ2000TDB();
        double M = MeanAnomaly() + MeanMotion() * ellapsedTime;
        while (M < 0.0)
        {
            M += Constants._2PI;
        }

        return new KeplerianElements(SemiMajorAxis(), Eccentricity(), Inclination(), AscendingNode(), ArgumentOfPeriapsis(), M % Constants._2PI, CenterOfMotion, epoch, Frame);
    }

    public virtual KeplerianElements ToKeplerianElements()
    {
        return ToKeplerianElements(Epoch);
    }

    public OrbitalParameters ToFrame(Frame frame)
    {
        if (frame == Frame)
        {
            return this;
        }

        StateVector icrfSv = ToStateVector();
        var orientation = Frame.ToFrame(frame, Epoch);
        var newPos = icrfSv.Position.Rotate(orientation.Rotation);
        var newVel = icrfSv.Velocity.Rotate(orientation.Rotation) - orientation.AngularVelocity.Cross(newPos);
        return new StateVector(newPos, newVel, CenterOfMotion, Epoch, frame);
    }

    public Equatorial ToEquatorial()
    {
        return new Equatorial(ToStateVector());
    }

    public double PerigeeVelocity()
    {
        return System.Math.Sqrt(CenterOfMotion.GM * (2 / PerigeeVector().Magnitude() - 1.0 / SemiMajorAxis()));
    }

    public double ApogeeVelocity()
    {
        return System.Math.Sqrt(CenterOfMotion.GM * (2 / ApogeeVector().Magnitude() - 1.0 / SemiMajorAxis()));
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as OrbitalParameters);
    }

    public virtual bool Equals(OrbitalParameters other)
    {
        return other is not null &&
            base.Equals(other) || (ToStateVector() == other?.ToStateVector());
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), CenterOfMotion, Epoch, Frame);
    }

    public static bool operator ==(OrbitalParameters left, OrbitalParameters right)
    {
        return EqualityComparer<OrbitalParameters>.Default.Equals(left, right);
    }

    public static bool operator !=(OrbitalParameters left, OrbitalParameters right)
    {
        return !(left == right);
    }
}