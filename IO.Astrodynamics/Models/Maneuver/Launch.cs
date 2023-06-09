using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models.Mission;
using IO.Astrodynamics.Models.Surface;
using IO.Astrodynamics.Models.Time;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IO.Astrodynamics.Models.Frames;

namespace IO.Astrodynamics.Models.Maneuver
{
    public class Launch
    {
        public LaunchSite LaunchSite { get; }
        public Site RecoverySite { get; }
        public OrbitalParameters.OrbitalParameters TargetOrbit { get; }
        public Mission.BodyScenario TargetBody { get; }
        public bool? LaunchByDay { get; }
        public double Twilight { get; }
        private readonly API _api = new API();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="launchSite">Launch from</param>
        /// <param name="recoverySite">Recovery site</param>
        /// <param name="targetBody">Body in orbit to reach</param>
        /// <param name="launchByDay">Define if launch should occur by day. If undefined launch can occur everytime</param>
        /// <exception cref="ArgumentNullException"></exception>
        public Launch(LaunchSite launchSite, Site recoverySite, BodyScenario targetBody, double twilight,
            bool? launchByDay)
        {
            LaunchSite = launchSite ?? throw new ArgumentNullException(nameof(launchSite));
            RecoverySite = recoverySite ?? throw new ArgumentNullException(nameof(recoverySite));
            TargetBody = targetBody ?? throw new ArgumentNullException(nameof(targetBody));
            LaunchByDay = launchByDay;
            Twilight = twilight;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="launchSite">Launch from</param>
        /// <param name="recoverySite">Recovery site</param>
        /// <param name="targetOrbit">Orbit to reach</param>
        /// <param name="launchByDay">Define if launch should occur by day. If undefined launch can occur everytime</param>
        /// <exception cref="ArgumentNullException"></exception>
        public Launch(LaunchSite launchSite, Site recoverySite, OrbitalParameters.OrbitalParameters targetOrbit,
            double twilight, bool? launchByDay)
        {
            LaunchSite = launchSite ?? throw new ArgumentNullException(nameof(launchSite));
            RecoverySite = recoverySite ?? throw new ArgumentNullException(nameof(recoverySite));
            TargetOrbit = targetOrbit ?? throw new ArgumentNullException(nameof(targetOrbit));
            LaunchByDay = launchByDay;
            Twilight = twilight;
        }

        OrbitalParameters.OrbitalParameters GetOrbitalParameters(in DateTime epoch)
        {
            if (TargetBody != null)
            {
                return TargetBody.GetEphemeris(epoch);
            }

            return TargetOrbit.AtEpoch(epoch);
        }

        double GetDeltaL(in DateTime epoch)
        {
            return System.Math.Asin(System.Math.Tan(LaunchSite.Geodetic.Latitude) /
                                    System.Math.Tan(GetInclination(epoch)));
        }

        double GetInclination(in DateTime epoch)
        {
            return Vector3.VectorZ.Rotate(LaunchSite.Body.Frame.ToFrame(Frame.ICRF, epoch).Rotation)
                .Angle(GetOrbitalParameters(epoch).ToFrame(Frame.ICRF).SpecificAngularMomentum());
        }

        public double GetInertialAscendingAzimuthLaunch(in DateTime epoch)
        {
            double azimuth = System.Math.Asin(System.Math.Cos(GetInclination(epoch)) /
                                              System.Math.Cos(LaunchSite.Geodetic.Latitude));
            if (azimuth < 0.0)
            {
                azimuth += System.Math.Tau;
            }

            return azimuth;
        }

        public double GetInertialDescendingAzimuthLaunch(in DateTime epoch)
        {
            double azimuth = Constants.PI - GetInertialAscendingAzimuthLaunch(epoch);
            if (azimuth < 0.0)
            {
                azimuth += System.Math.Tau;
            }

            return azimuth;
        }

        public double GetNonInertialAscendingAzimuthLaunch(in DateTime epoch)
        {
            double vrotx =
                GetInertialInsertionVelocity(epoch) * System.Math.Sin(GetInertialAscendingAzimuthLaunch(epoch)) -
                LaunchSite.GetEphemeris(Frames.Frame.ICRF, epoch).Velocity.Magnitude();
            double vroty = GetInertialInsertionVelocity(epoch) *
                           System.Math.Cos(GetInertialAscendingAzimuthLaunch(epoch));
            double az = System.Math.Atan(vrotx / vroty);
            if (az < 0.0)
            {
                az += Constants._2PI;
            }

            return az;
        }

        public double GetNonInertialDescendingAzimuthLaunch(in DateTime epoch)
        {
            var az = Constants.PI - GetNonInertialAscendingAzimuthLaunch(epoch);
            if (az < 0.0)
            {
                az += Constants._2PI;
            }

            return az;
        }

        public double GetInertialInsertionVelocity(in DateTime epoch)
        {
            return GetOrbitalParameters(epoch).PerigeeVelocity();
        }

        public double GetNonInertialInsertionVelocity(in DateTime epoch)
        {
            double vrotx =
                GetInertialInsertionVelocity(epoch) * System.Math.Sin(GetInertialAscendingAzimuthLaunch(epoch)) -
                LaunchSite.GetEphemeris(Frames.Frame.ICRF, epoch).Velocity.Magnitude();
            double vroty = GetInertialInsertionVelocity(epoch) *
                           System.Math.Cos(GetInertialAscendingAzimuthLaunch(epoch));
            return System.Math.Sqrt(vrotx * vrotx + vroty * vroty);
        }

        /// <summary>
        /// Find launch windows based on launch's constraints in the given window
        /// </summary>
        /// <param name="searchWindow"></param>
        /// <param name="outputDirectory"></param>
        /// <returns></returns>
        public IEnumerable<LaunchWindow> FindLaunchWindows(in Window searchWindow, DirectoryInfo outputDirectory)
        {
            return _api.FindLaunchWindows(this, searchWindow, outputDirectory);
        }
    }
}