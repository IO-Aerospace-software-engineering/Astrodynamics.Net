using System;
using System.Collections.Generic;
using IO.Astrodynamics.Models.Body;
using IO.Astrodynamics.Models.Coordinates;
using IO.Astrodynamics.Models.Frames;
using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models.Mission;
using IO.Astrodynamics.Models.Time;

namespace IO.Astrodynamics.Models.Surface
{
    public class Site : ILocalizable, INaifObject
    {
        public int Id { get; }
        public int NaifId { get; }
        public string Name { get; }
        public CelestialBody Body { get; }
        public Geodetic Geodetic { get; }
        private readonly API _api = new API();

        public Site(int id, string name, CelestialBody body, in Geodetic geodetic)
        {
            if (body == null) throw new ArgumentNullException(nameof(body));
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            if (body == null) throw new ArgumentNullException(nameof(body));
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Value cannot be null or empty.", nameof(name));
            Name = name;
            Body = body;
            Geodetic = geodetic;
            Id = id;
            NaifId = body.NaifId * 1000 + id;
        }
        
        public Site(int id, string name, CelestialBody body)
        {
            if (body == null) throw new ArgumentNullException(nameof(body));
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            if (body == null) throw new ArgumentNullException(nameof(body));
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Value cannot be null or empty.", nameof(name));
            Name = name;
            Body = body;
            Id = id;
            NaifId = body.NaifId * 1000 + id;
        }

        /// <summary>
        /// Get horizontal coordinates
        /// </summary>
        /// <param name="epoch"></param>
        /// <param name="localizableObject"></param>
        /// <param name="aberration"></param>
        /// <returns></returns>
        public Horizontal GetHorizontalCoordinates(DateTime epoch, ILocalizable localizableObject, Aberration aberration)
        {
            var bodySv = localizableObject.GetEphemeris(epoch, Body, Body.Frame, aberration).ToStateVector();
            var r = bodySv.Position.Normalize();
            var z = GetEphemeris(epoch, Body, Body.Frame, aberration).ToStateVector().Position.Normalize();
            var e = z.Cross(Vector3.VectorZ).Normalize().Inverse();
            var n = z.Cross(e).Normalize().Inverse();

            var az = System.Math.Atan((r * e) / (r * n));
            var el = System.Math.Asin(r * z);

            return new Horizontal(az, el, bodySv.Position.Magnitude());
        }

        public IEnumerable<OrbitalParameters.OrbitalParameters> GetEphemeris(Window searchWindow, CelestialBody observer, Frame frame, Aberration aberration,
            TimeSpan stepSize)
        {
            return _api.ReadEphemeris(searchWindow, observer, this, frame, aberration, stepSize);
        }

        public OrbitalParameters.OrbitalParameters GetEphemeris(DateTime epoch, CelestialBody observer, Frame frame, Aberration aberration)
        {
            return _api.ReadEphemeris(epoch, observer, this, frame, aberration);
        }
    }
}