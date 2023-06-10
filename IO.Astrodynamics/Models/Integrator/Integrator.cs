using IO.Astrodynamics.Models.Mission;
using IO.Astrodynamics.Models.OrbitalParameters;
using System;

namespace IO.Astrodynamics.Models.Integrator
{
    public abstract class Integrator
    {
        public BodyScenario Body { get; private set; }
        public TimeSpan IntegrationStep { get; private set; }
        protected TimeSpan HalfIntegrationStep { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="body"></param>
        /// <param name="integrationStep"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        protected Integrator(BodyScenario body, TimeSpan integrationStep)
        {
            Body = body ?? throw new ArgumentNullException(nameof(body));
            if (Body.InitialOrbitalParameters == null)
            {
                throw new ArgumentException("Body must have orbital parameters");
            }
            IntegrationStep = integrationStep;
            HalfIntegrationStep = integrationStep * 0.5;
        }

        /// <summary>
        /// Change the current body used by the integrator
        /// </summary>
        /// <param name="bodyScenario"></param>
        public void UpdateBody(BodyScenario bodyScenario)
        {
            Body = bodyScenario ?? throw new ArgumentNullException(nameof(bodyScenario));
        }

        public abstract StateVector Integrate();
    }
}