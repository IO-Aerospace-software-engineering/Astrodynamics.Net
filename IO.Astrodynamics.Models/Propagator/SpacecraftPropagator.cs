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
            StateVector sv = Body.GetLatestEphemeris();

            if (!Body.Scenario.Window.Intersects(sv.Epoch))
            {
                throw new InvalidOperationException("Initial state vector epoch is out of scenario Window ");
            }
            
            while (sv.Epoch <= Body.Scenario.Window.EndDate)
            {
                if (Body.StandbyManeuver != null)
                {
                    if (Body.StandbyManeuver.CanExecute(sv))
                    {
                        //If release occurs we set the new active body used by propagator and integrator
                        if (Body.StandbyManeuver is ReleaseManeuver)
                        {
                            Body = Body.Child;
                            _integrator.UpdateBody(Body);
                            continue;
                        }
                        Body.StandbyManeuver.Execute(sv);
                    }
                }
                sv = _integrator.Integrate();
                Body.AddStateVector(sv);
            }

        }
    }
}
