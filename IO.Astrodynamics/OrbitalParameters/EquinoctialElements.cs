using System;
using System.Collections.Generic;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.Frames;

namespace IO.Astrodynamics.OrbitalParameters
{
    public class EquinoctialElements : OrbitalParameters, IEquatable<EquinoctialElements>
    {
        

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p">P coefficient</param>
        /// <param name="f">F Coefficient</param>
        /// <param name="g">G Coefficient</param>
        /// <param name="h">H Coefficient</param>
        /// <param name="k">K Coefficient</param>
        /// <param name="l0">True longitude</param>
        /// <param name="centerOfMotion">Center of motion</param>
        /// <param name="epoch">Epoch</param>
        /// <param name="frame">Reference frame</param>
        /// <returns></returns>
        public EquinoctialElements(double p, double f, double g, double h, double k, double l0, CelestialBody centerOfMotion, DateTime epoch, Frame frame) : base(centerOfMotion,
            epoch, frame)
        {
            P = p;
            F = f;
            G = g;
            H = h;
            K = k;
            L0 = l0;
        }

        public double P { get; }
        public double F { get; }
        public double G { get; }
        public double H { get; }
        public double K { get; }
        public double L0 { get; }

        public override double ArgumentOfPeriapsis()
        {
            return System.Math.Atan2(G * H - F * K, F * H + G * K);
        }

        public override double AscendingNode()
        {
            return System.Math.Atan(K / H);
        }

        public override double EccentricAnomaly()
        {
            double v = TrueAnomaly();
            double e = Eccentricity();
            return 2 * System.Math.Atan((System.Math.Tan(v / 2.0)) / System.Math.Sqrt((1 + e) / (1 - e)));
        }

        public override double Eccentricity()
        {
            return System.Math.Sqrt(F * F + G * G);
        }

        public override double Inclination()
        {
            return System.Math.Atan2(2 * System.Math.Sqrt(H * H + K * K), 1 - H * H - K * K);
        }

        public override double MeanAnomaly()
        {
            return EccentricAnomaly() - Eccentricity() * System.Math.Sin(EccentricAnomaly());
        }

        public override double SemiMajorAxis()
        {
            return P / (1 - F * F - G * G);
        }

        public override double TrueAnomaly()
        {
            return L0 - (AscendingNode() + ArgumentOfPeriapsis());
        }

        public override EquinoctialElements ToEquinoctial()
        {
            return this;
        }
        
        public bool Equals(EquinoctialElements other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return P.Equals(other.P) && F.Equals(other.F) && G.Equals(other.G) && H.Equals(other.H) && K.Equals(other.K) && L0.Equals(other.L0) &&
                   CenterOfMotion == other.CenterOfMotion && Epoch == other.Epoch && Frame == other.Frame;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((EquinoctialElements)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), P, F, G, H, K, L0);
        }

        public static bool operator ==(EquinoctialElements left, EquinoctialElements right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(EquinoctialElements left, EquinoctialElements right)
        {
            return !Equals(left, right);
        }
    }
}