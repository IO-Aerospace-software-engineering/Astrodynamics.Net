
using IO.Astrodynamics.Models.Time;

namespace IO.Astrodynamics.Models.Maneuver
{
    public class LaunchWindow 
    {
        public Window Window { get; private set; }
        public double InertialInsertionVelocity { get; private set; }
        public double NonInertialInsertionVelocity { get; private set; }
        public double InertialAzimuth { get; private set; }
        public double NonInertialAzimuth { get; private set; }

        public LaunchWindow(Window window, double inertialInsertionVelocity, double nonInertialInsertionVelocity, double inertialAzimuth, double nonInertialAzimuth)
        {
            Window = window;
            InertialInsertionVelocity = inertialInsertionVelocity;
            NonInertialInsertionVelocity = nonInertialInsertionVelocity;
            InertialAzimuth = inertialAzimuth;
            NonInertialAzimuth = nonInertialAzimuth;
        }
    }
}
