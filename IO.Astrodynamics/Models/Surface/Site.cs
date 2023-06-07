using IO.Astrodynamics.Models.Body;
using IO.Astrodynamics.Models.Coordinates;
using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models.Mission;
using IO.Astrodynamics.Models.OrbitalParameters;

using IO.Astrodynamics.Models.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IO.Astrodynamics.Models.Surface
{
    public class Site : ILocalizable
    {
        public string Name { get; private set; }
        public CelestialBodyScenario Body { get; }
        public Geodetic Geodetic { get; }
        

        protected Site() { }

        public Site(string name, CelestialBodyScenario body, in Geodetic geodetic) 
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Body = body ?? throw new ArgumentNullException(nameof(body));
            Geodetic = geodetic;
        }

        /// <summary>
        /// Get ephemeris at epoch in given frame
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="epoch"></param>
        /// <param name="accuracy"></param>
        /// <returns></returns>
        public StateVector GetEphemeris(Frames.Frame frame, in DateTime epoch, int accuracy = 9)
        {
            return StateVector(epoch).ToFrame(frame).ToStateVector();
        }

        /// <summary>
        /// Get relative state vector in given frame
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="targetBody"></param>
        /// <param name="epoch"></param>
        /// <returns></returns>
        public StateVector RelativeStateVector(Frames.Frame frame, BodyScenario targetBody, in DateTime epoch)
        {
            var relativeBody = Body.RelativeStateVector(targetBody, epoch).ToFrame(frame).ToStateVector();
            var relativeSite = StateVector(epoch).ToFrame(frame).ToStateVector();
            return relativeBody - relativeSite;
        }


        /// <summary>
        /// Get state vector relative to body frame
        /// </summary>
        /// <param name="epoch"></param>
        /// <returns></returns>
        private StateVector StateVector(DateTime epoch)
        {
            double e = 2 * Body.PhysicalBody.Flatenning - Body.PhysicalBody.Flatenning * Body.PhysicalBody.Flatenning;
            double e2 = e * e;
            double sinLat = System.Math.Sin(Geodetic.Latitude);
            double sinLat2 = sinLat * sinLat;
            double cosLat = System.Math.Cos(Geodetic.Latitude);

            double N = Body.PhysicalBody.EquatorialRadius / System.Math.Sqrt(1 - e2 * sinLat2);

            double x = (N + Geodetic.Altitude) * cosLat * System.Math.Cos(Geodetic.Longitude);
            double y = (N + Geodetic.Altitude) * cosLat * System.Math.Sin(Geodetic.Longitude);
            double z = ((1.0 - e2) * N + Geodetic.Altitude) * sinLat;

            return new StateVector(new Math.Vector3(x, y, z), new Math.Vector3(), Body, epoch, Body.Frame);
        }

        public Window[] FindNightWindows(in Window window, in double twilight)
        {
            var bydays = FindDayWindows(window, twilight);
            List<Window> windows = new List<Window>();
            if (bydays.Length == 0)
            {
                windows.Add(window);
                return windows.ToArray();
            }

            if (window.StartDate < bydays[0].StartDate)
            {
                windows.Add(new Window(window.StartDate, bydays[0].StartDate));
            }

            for (int i = 0; i < bydays.Length - 1; i++)
            {
                windows.Add(new Window(bydays[i].EndDate, bydays[i + 1].StartDate));
            }

            if (window.EndDate > bydays.Last().EndDate)
            {
                windows.Add(new Window(bydays.Last().EndDate, window.EndDate));
            }

            return windows.ToArray();
        }
        public Window[] FindDayWindows(in Window window, in double twilight)
        {
            //define body rotation duration
            TimeSpan coarseStepSize = TimeSpan.FromSeconds(Constants._2PI / Body.GetOrientationFromICRF(window.StartDate).AngularVelocity.Magnitude() * 0.25);
            TimeSpan stepSize = coarseStepSize;

            var epoch = window.StartDate;
            bool isDay = IsDay(epoch, twilight);

            List<Window> windows = new List<Window>();

            while (epoch <= window.EndDate)
            {
                DateTime? start = isDay ? epoch : null;
                DateTime? end = null;
                while (System.Math.Abs(stepSize.TotalSeconds) > 1.0)
                {
                    epoch += stepSize;
                    if (isDay != IsDay(epoch, twilight))
                    {
                        stepSize *= -0.5;
                        isDay = !isDay;
                    }
                }

                if (start.HasValue)
                {
                    end = epoch;
                    windows.Add(new Window(start.Value, end.Value));
                    isDay = false;

                }
                else
                {
                    isDay = true;
                }
                stepSize = coarseStepSize;
            }

            return windows.ToArray();
        }

        private CelestialBodyScenario GetSun()
        {
            //get sun
            CelestialBodyScenario sun = null;
            CelestialBodyScenario body = Body.InitialOrbitalParameters.CenterOfMotion;
            while (body != null)
            {
                if (body.PhysicalBody.NaifId == 10)
                {
                    sun = body;
                    break;
                }
                body = body.InitialOrbitalParameters.CenterOfMotion;
            }

            if (sun == null)
            {
                throw new InvalidOperationException("Sun not found");
            }
            return sun;
        }

        public bool IsDay(in DateTime epoch, in double twilight)
        {
            var angle = GetEphemeris(Frames.Frame.ICRF, epoch).Position.Inverse().Angle(RelativeStateVector(Frames.Frame.ICRF, GetSun(), epoch).Position);

            return angle > Constants.PI2 - twilight;
        }

        public bool IsNight(in DateTime epoch, in double twilight)
        {
            return !IsDay(epoch, twilight);
        }

        /// <summary>
        /// Get horizontal coordinates
        /// </summary>
        /// <param name="target"></param>
        /// <param name="epoch"></param>
        /// <returns></returns>
        public Horizontal GetHorizontalCoordinates(BodyScenario target, DateTime epoch)
        {
            var bodySv = RelativeStateVector(this.Body.Frame, target, epoch);
            var r = bodySv.Position.Normalize();
            var z = StateVector(epoch).Position.Normalize();
            var e = z.Cross(Vector3.VectorZ).Normalize().Inverse();
            var n = z.Cross(e).Normalize().Inverse();

            var az = System.Math.Atan((r * e) / (r * n));
            var el = System.Math.Asin(r * z);

            return new Horizontal(az, el, bodySv.Position.Magnitude());
        }


    }
}
