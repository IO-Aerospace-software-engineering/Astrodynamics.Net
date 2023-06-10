using IO.Astrodynamics.Models.Mission;

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
