using IO.Astrodynamics.Models.Integrator;
using IO.Astrodynamics.Models.Mission;
using IO.Astrodynamics.Models.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IO.Astrodynamics.Models.Propagator
{
    public class BodyPropagator
    {
        protected readonly Integrator.Integrator _integrator = null;
        public BodyScenario Body { get; }
        public BodyPropagator(Integrator.Integrator integrator, BodyScenario body)
        {
            _integrator = integrator ?? throw new ArgumentNullException(nameof(integrator));
            Body = body ?? throw new ArgumentNullException(nameof(body));
        }

        public virtual void Propagate()
        {
            while (Body.GetLatestEphemeris().Epoch <= Body.Scenario.Window.EndDate)
            {
                Body.AddStateVector(_integrator.Integrate());
            }
        }
    }
}
