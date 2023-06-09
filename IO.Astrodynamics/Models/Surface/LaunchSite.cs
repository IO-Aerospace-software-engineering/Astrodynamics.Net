using IO.Astrodynamics.Models.Coordinates;
using IO.Astrodynamics.Models.Mission;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IO.Astrodynamics.Models.Surface
{
    public class LaunchSite : Site
    {
        public IReadOnlyCollection<AzimuthRange> AzimuthRanges => _azimuthRanges;
        private readonly List<AzimuthRange> _azimuthRanges;

        /// <summary>
        /// Create launch site
        /// </summary>
        /// <param name="name">Site name</param>
        /// <param name="body">Celestial body</param>
        /// <param name="geodetic">Geodetic coordinates</param>
        /// <param name="launchAzimuths">Allowed launch azimuths</param>
        public LaunchSite(int id, string name, CelestialBodyScenario body, in Geodetic geodetic, params AzimuthRange[] launchAzimuths) : base(id, name,
            body, geodetic)
        {
            _azimuthRanges = new List<AzimuthRange>(launchAzimuths);
        }

        /// <summary>
        /// Know if the azimuth is allowed
        /// </summary>
        /// <param name="azimuth"></param>
        /// <returns></returns>
        public bool IsAzimuthAllowed(double azimuth)
        {
            return _azimuthRanges.Any(x => x.IsInRange(azimuth));
        }
    }
}