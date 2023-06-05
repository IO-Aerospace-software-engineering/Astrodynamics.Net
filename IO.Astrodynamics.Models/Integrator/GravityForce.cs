using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models.Mission;
using IO.Astrodynamics.Models.OrbitalParameters;

namespace IO.Astrodynamics.Models.Integrator
{
    public class GravityForce : Force
    {
        public override Vector3 Apply(BodyScenario body, StateVector sv)
        {
            //Compute force from his major body
            var currentCenterOfMotion = sv.CenterOfMotion;
            Vector3 position = sv.Position;
            double mass = body.PhysicalBody.GetTotalMass();
            Vector3 force = ComputeForce(currentCenterOfMotion.PhysicalBody.Mass, mass, position.Magnitude(), position.Normalize());

            //Each body is under sphere of influence of his major body
            //So spacecraft is influenced by his center of motion and his parents
            //Eg. Sun->Earth->Moon->Spacecraft
            while (currentCenterOfMotion?.InitialOrbitalParameters != null)
            {
                //Compute vector state
                position += currentCenterOfMotion.GetEphemeris(sv.Epoch).Position;

                //Compute force
                force += ComputeForce(currentCenterOfMotion.InitialOrbitalParameters.CenterOfMotion.PhysicalBody.Mass, mass, position.Magnitude(), position.Normalize());

                //Set next parent
                currentCenterOfMotion = currentCenterOfMotion.InitialOrbitalParameters?.CenterOfMotion;
            }

            foreach (var sat in body.InitialOrbitalParameters.CenterOfMotion.Satellites)
            {
                if (sat is not CelestialBodyScenario || sat == body)
                {
                    continue;
                }

                var satSv = sat.GetEphemeris(sv.Epoch);
                var relativePosition = sv.Position - satSv.Position;
                force += ComputeForce(body.InitialOrbitalParameters.CenterOfMotion.PhysicalBody.Mass, mass, relativePosition.Magnitude(), relativePosition.Normalize());
            }

            return force;
        }

        public static Vector3 ComputeForce(double m1, double m2, double distance, Vector3 u12)
        {
            return (u12 * (-Constants.G * ((m1 * m2) / (distance * distance)))) * 1E+03;
        }
    }
}