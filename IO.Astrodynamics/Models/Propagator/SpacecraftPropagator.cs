using IO.Astrodynamics.Models.Maneuver;
using IO.Astrodynamics.Models.Mission;
using IO.Astrodynamics.Models.OrbitalParameters;
using IO.Astrodynamics.Models.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IO.Astrodynamics.Models.Propagator
{
    public class SpacecraftPropagator : BodyPropagator
    {
        new public SpacecraftScenario Body { get; private set; }
        public SpacecraftPropagator(Integrator.VVIntegrator integrator, SpacecraftScenario body) : base(integrator, body)
        {
            Body = body;
        }

        public override void Propagate()
        {

        }
    }
}
