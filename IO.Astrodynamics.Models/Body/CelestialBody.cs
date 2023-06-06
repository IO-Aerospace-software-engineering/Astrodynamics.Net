

using System;
using IO.Astrodynamics.Models.OrbitalParameters;

namespace IO.Astrodynamics.Models.Body;
public class CelestialBody : Body
{
    public const int SunNaifId = 10;
    public string FrameName { get; private set; }
    public double PolarRadius { get; private set; }
    public double EquatorialRadius { get; private set; }
    public double Flattening { get; private set; }
    public double GM { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="naifId"></param>
    /// <param name="name"></param>
    /// <param name="GM">Km3/s2</param>
    /// <param name="polarRadius"></param>
    /// <param name="equatorialRadius"></param>
    /// <returns></returns>
    public CelestialBody(int naifId, string name, double GM, double polarRadius, double equatorialRadius) : this(naifId, name, GM, polarRadius, equatorialRadius, "IAU_" + name.ToUpper())
    {

    }

    public CelestialBody(int naifId, string name, double GM, double polarRadius, double equatorialRadius, string frameName) : base(naifId, name, GM / Constants.G)
    {
        if (naifId < 0)
        {
            throw new ArgumentException("Naif Id must be a positive number");
        }
        if (GM < 0.0)
        {
            throw new ArgumentException("Invalid GM");
        }
        if (polarRadius < 0.0)
        {
            throw new ArgumentException("Invalid polar Radius");
        }
        if (equatorialRadius < 0.0)
        {
            throw new ArgumentException("Invalid equatorial Radius");
        }

        FrameName = frameName;
        PolarRadius = polarRadius;
        EquatorialRadius = equatorialRadius;
        Flattening = (equatorialRadius - polarRadius) / equatorialRadius;
        this.GM = GM;
        if (double.IsNaN(Flattening))
        {
            Flattening = double.PositiveInfinity;
        }
    }



    public override double GetTotalMass()
    {
        return Mass;
    }
}