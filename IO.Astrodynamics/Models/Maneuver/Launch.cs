using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models.Mission;

using IO.Astrodynamics.Models.Surface;
using IO.Astrodynamics.Models.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IO.Astrodynamics.Models.Maneuver
{
    public class Launch 
    {
        public LaunchSite LaunchSite { get; private set; }
        public Site RecoverySite { get; private set; }
        public OrbitalParameters.OrbitalParameters TargetOrbit { get; private set; }
        public Mission.BodyScenario TargetBody { get; private set; }
        public bool? LaunchByDay { get; private set; }
        public double Twilight { get; private set; }

        Launch()
        {
        }

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
            return Vector3.VectorZ.Rotate(LaunchSite.Body.Frame.ToICRF(epoch).Orientation)
                .Angle(GetOrbitalParameters(epoch).ToFrame(Frames.Frame.ICRF).SpecificAngularMomentum());
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
        /// <returns></returns>
        public LaunchWindow[] FindLaunchWindows(in Window searchWindow)
        {
            //if not specified return all windows
            if (!LaunchByDay.HasValue)
            {
                return SearchLaunchWindows(searchWindow);
            }

            List<Window> launchSiteIlluminationWindows = new List<Window>();
            List<Window> recoverySiteIlluminationWindows = new List<Window>();
            List<Window> commonIlluminationWindows = new List<Window>();

            if (LaunchByDay != false) //Find sites windows by days
            {
                launchSiteIlluminationWindows.AddRange(LaunchSite.FindDayWindows(searchWindow, Twilight));
                recoverySiteIlluminationWindows.AddRange(RecoverySite.FindDayWindows(searchWindow, Twilight));
            }

            if (LaunchByDay != true) //Find sites windows by night
            {
                launchSiteIlluminationWindows.AddRange(LaunchSite.FindNightWindows(searchWindow, Twilight));
                recoverySiteIlluminationWindows.AddRange(RecoverySite.FindNightWindows(searchWindow, Twilight));
            }

            //Find common illumination windows
            foreach (var launchSiteWindow in launchSiteIlluminationWindows)
            {
                foreach (var recoverySiteWindow in recoverySiteIlluminationWindows)
                {
                    if (launchSiteWindow.Intersects(recoverySiteWindow))
                    {
                        commonIlluminationWindows.Add(launchSiteWindow.GetIntersection(recoverySiteWindow));
                    }
                }
            }

            //Find launch windows from illumination windows
            List<LaunchWindow> launchWindows = new List<LaunchWindow>();
            foreach (var commonIlluminationWindow in commonIlluminationWindows)
            {
                launchWindows.AddRange(SearchLaunchWindows(commonIlluminationWindow));
            }

            return launchWindows.ToArray();
        }

        /// <summary>
        /// Search launch windows in given window
        /// </summary>
        /// <param name="searchWindow"></param>
        /// <returns></returns>
        private LaunchWindow[] SearchLaunchWindows(in Window searchWindow)
        {
            var date = searchWindow.StartDate;
            var step = TimeSpan.FromSeconds(Constants._2PI /
                LaunchSite.Body.GetOrientationFromICRF(date).AngularVelocity.Magnitude() * 0.25);
            var initialStep = step;
            List<LaunchWindow> launchWindows = new List<LaunchWindow>();

            while (date < searchWindow.EndDate)
            {
                bool status = LaunchSite.GetEphemeris(Frames.Frame.ICRF, date).Position *
                    GetOrbitalParameters(date).ToFrame(Frames.Frame.ICRF).SpecificAngularMomentum() > 0.0;
                while (System.Math.Abs(step.TotalSeconds) > 1.0)
                {
                    date += step;
                    if (status != LaunchSite.GetEphemeris(Frames.Frame.ICRF, date).Position * GetOrbitalParameters(date)
                            .ToFrame(Frames.Frame.ICRF).SpecificAngularMomentum() > 0.0)
                    {
                        step *= -0.5;
                        status = !status;
                    }
                }

                if (date > searchWindow.EndDate)
                {
                    break;
                }

                double inertialVel = GetInertialInsertionVelocity(date);
                double nonInertialVel = GetNonInertialInsertionVelocity(date);
                double inertialAz = double.NaN;
                double nonInertialAz = double.NaN;
                bool isAscending = LaunchSite.GetEphemeris(Frames.Frame.ICRF, date).Position *
                    GetOrbitalParameters(date).ToFrame(Frames.Frame.ICRF).AscendingNodeVector() > 0.0;
                if (isAscending)
                {
                    inertialAz = GetInertialAscendingAzimuthLaunch(date);
                    nonInertialAz = GetNonInertialAscendingAzimuthLaunch(date);
                }
                else
                {
                    inertialAz = GetInertialDescendingAzimuthLaunch(date);
                    nonInertialAz = GetNonInertialDescendingAzimuthLaunch(date);
                }


                launchWindows.Add(new LaunchWindow(new Window(date, date), inertialVel, nonInertialVel, inertialAz,
                    nonInertialAz));
                step = initialStep;
                date = date.AddSeconds(2.0);
            }

            return launchWindows.ToArray();
        }
    }
}