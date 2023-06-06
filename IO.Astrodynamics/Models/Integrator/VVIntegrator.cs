using IO.Astrodynamics.Models.Integrator;
using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models.Mission;
using IO.Astrodynamics.Models.OrbitalParameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IO.Astrodynamics.Models.Integrator
{
    public class VVIntegrator : Integrator
    {
        private readonly IReadOnlyCollection<Force> _forces;
        private Vector3? _acceleration = null;

        public VVIntegrator(BodyScenario body, TimeSpan integrationStep, params Force[] forces) : base(body, integrationStep)
        {
            _forces = forces;
        }

        public override StateVector Integrate()
        {
            StateVector stateVector = Body.GetLatestEphemeris().ToStateVector();
            var position = stateVector.Position;
            var velocity = stateVector.Velocity;
            var nextEpoch = stateVector.Epoch + IntegrationStep;

            if (_acceleration is null)
            {
                _acceleration = ComputeAcceleration(stateVector);
            }

            velocity += _acceleration.Value * 1E-03 * HalfIntegrationStep.TotalSeconds;
            position += velocity * IntegrationStep.TotalSeconds;

            _acceleration = ComputeAcceleration(new StateVector(position, velocity, stateVector.CenterOfMotion, nextEpoch, stateVector.Frame));
            velocity += _acceleration.Value * 1E-03 * HalfIntegrationStep.TotalSeconds;

            //Check if body has a new center of motion
            var sv = new StateVector(position, velocity, stateVector.CenterOfMotion, nextEpoch, stateVector.Frame);

            sv.CheckAndUpdateCenterOfMotion(Body);

            return sv;
        }

        private Vector3 ComputeAcceleration(StateVector sv)
        {
            Vector3 forces = new Vector3();
            foreach (var force in _forces)
            {
                forces += force.Apply(Body, sv);
            }
            return forces / Body.PhysicalBody.GetTotalMass();
        }
    }
}
