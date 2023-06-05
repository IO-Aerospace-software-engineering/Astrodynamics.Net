using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models.Mission;
using IO.Astrodynamics.Models.OrbitalParameters;

namespace IO.Astrodynamics.Models.Integrator
{
    public abstract class Force
    {
        public abstract Vector3 Apply(BodyScenario body, StateVector sv);
    }
}