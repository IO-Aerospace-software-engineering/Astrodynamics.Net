using System;
using System.Collections.Generic;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.Frames;


namespace IO.Astrodynamics.OrbitalParameters
{
    public class KeplerianElements : OrbitalParameters, IEquatable<KeplerianElements>
    {
        

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="semiMajorAxis"></param>
        /// <param name="eccentricity"></param>
        /// <param name="inclination"></param>
        /// <param name="rigthAscendingNode"></param>
        /// <param name="argumentOfPeriapsis"></param>
        /// <param name="meanAnomaly"></param>
        /// <param name="observer"></param>
        /// <param name="epoch"></param>
        /// <param name="frame"></param>
        /// <exception cref="ArgumentException"></exception>
        public KeplerianElements(double semiMajorAxis, double eccentricity, double inclination, double rigthAscendingNode, double argumentOfPeriapsis, double meanAnomaly,
            ILocalizable observer, DateTime epoch, Frame frame) : base(observer, epoch, frame)
        {
            if (semiMajorAxis <= 0.0)
            {
                throw new ArgumentException("Semi major axis must be a positive number");
            }

            if (eccentricity < 0.0)
            {
                throw new ArgumentException("Eccentricity must be a positive number");
            }

            if (inclination < -Constants.PI || inclination > Constants.PI)
            {
                throw new ArgumentException("Inclination must be in range [-PI,PI] ");
            }

            if (rigthAscendingNode < 0.0 || rigthAscendingNode > Constants._2PI)
            {
                throw new ArgumentException("Rigth ascending node must be in range [0.0,2*PI] ");
            }

            if (argumentOfPeriapsis < 0.0 || argumentOfPeriapsis > Constants._2PI)
            {
                throw new ArgumentException("Argument of periapsis must be in range [0.0,2*PI] ");
            }

            if (meanAnomaly < 0.0 || meanAnomaly > Constants._2PI)
            {
                throw new ArgumentException("Mean anomaly must be in range [0.0,2*PI] ");
            }

            A = semiMajorAxis;
            E = eccentricity;
            I = inclination;
            RAAN = rigthAscendingNode;
            AOP = argumentOfPeriapsis;
            M = meanAnomaly;
        }

        public double A { get; }
        public double E { get; }
        public double I { get; }
        public double RAAN { get; }
        public double AOP { get; }
        public double M { get; }

        public override double ArgumentOfPeriapsis()
        {
            return AOP;
        }

        public override double AscendingNode()
        {
            return RAAN;
        }

        public override double EccentricAnomaly()
        {
            double tmpEA = M;
            double EA = 0.0;

            while (System.Math.Abs(tmpEA - EA) > 1E-09)
            {
                EA = tmpEA;
                tmpEA = M + E * System.Math.Sin(EA);
            }

            return EA;
        }

        public override double Eccentricity()
        {
            return E;
        }

        public override double Inclination()
        {
            return I;
        }

        public override double SemiMajorAxis()
        {
            return A;
        }

        public override double TrueAnomaly()
        {
            double EA = EccentricAnomaly();
            double v = System.Math.Atan2(System.Math.Sqrt(1 - E * E) * System.Math.Sin(EA), System.Math.Cos(EA) - E);
            if (v < 0.0)
            {
                v += Constants._2PI;
            }

            return v % Constants._2PI;
        }

        public override double MeanAnomaly()
        {
            return M;
        }

        public override KeplerianElements ToKeplerianElements()
        {
            return this;
        }
        
        public bool Equals(KeplerianElements other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return A.Equals(other.A) && E.Equals(other.E) && I.Equals(other.I) && RAAN.Equals(other.RAAN) && AOP.Equals(other.AOP) && M.Equals(other.M) &&
                   Observer.NaifId == other.Observer.NaifId && Epoch == other.Epoch && Frame == other.Frame;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((KeplerianElements)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), A, E, I, RAAN, AOP, M);
        }

        public static bool operator ==(KeplerianElements left, KeplerianElements right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(KeplerianElements left, KeplerianElements right)
        {
            return !Equals(left, right);
        }
    }
}