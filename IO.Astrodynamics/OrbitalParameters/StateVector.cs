using System;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.Frames;
using IO.Astrodynamics.Math;

namespace IO.Astrodynamics.OrbitalParameters
{
    public class StateVector : OrbitalParameters, IEquatable<StateVector>
    {
        public Vector3 Position { get; internal set; }
        public Vector3 Velocity { get; internal set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="position"></param>
        /// <param name="velocity"></param>
        /// <param name="observer"></param>
        /// <param name="epoch"></param>
        /// <param name="frame"></param>
        public StateVector(Vector3 position, Vector3 velocity, ILocalizable observer, DateTime epoch, Frame frame) : base(observer, epoch, frame)
        {
            Position = position;
            Velocity = velocity;
        }

        public override Vector3 SpecificAngularMomentum()
        {
            return Position.Cross(Velocity);
        }

        public override double Eccentricity()
        {
            return EccentricityVector().Magnitude();
        }

        public override Vector3 EccentricityVector()
        {
            return (Velocity.Cross(SpecificAngularMomentum()) / Observer.GM) - (Position / Position.Magnitude());
        }

        public override double Inclination()
        {
            return SpecificAngularMomentum().Angle(Vector3.VectorZ);
        }


        public override double SpecificOrbitalEnergy()
        {
            return System.Math.Pow(Velocity.Magnitude(), 2.0) / 2.0 - (Observer.GM / Position.Magnitude());
        }

        public override double SemiMajorAxis()
        {
            return -(Observer.GM / (2.0 * SpecificOrbitalEnergy()));
        }

        public override Vector3 AscendingNodeVector()
        {
            if (Inclination() == 0.0)
            {
                return Vector3.VectorX;
            }

            var h = SpecificAngularMomentum();
            return new Vector3(-h.Y, h.X, 0.0);
        }

        public override double AscendingNode()
        {
            Vector3 n = AscendingNodeVector();

            var omega = System.Math.Acos(n.X / n.Magnitude());
            if (n.Y < 0.0)
            {
                omega = 2 * System.Math.PI - omega;
            }

            return omega;
        }

        public override double ArgumentOfPeriapsis()
        {
            var n = AscendingNodeVector();
            var e = EccentricityVector();
            var w = System.Math.Acos((n * e) / (n.Magnitude() * e.Magnitude()));
            if (e.Z < 0.0)
            {
                w = System.Math.PI * 2.0 - w;
            }

            return w;
        }

        public override double TrueAnomaly()
        {
            if (IsCircular())
            {
                if (Inclination() < 1E-03)
                {
                    return CircularNoInclinationTrueAnomaly();
                }

                return CircularTrueAnomaly();
            }
            var e = EccentricityVector();
            var v = System.Math.Acos((e * Position) / (e.Magnitude() * Position.Magnitude()));
            if (Position * Velocity < 0.0)
            {
                v = System.Math.PI * 2.0 - v;
            }

            return v;
        }

        private double CircularTrueAnomaly()
        {
            var omega = AscendingNodeVector();
            var v = System.Math.Acos((omega * Position) / (omega.Magnitude() * Position.Magnitude()));
            if (Position.Z < 0.0)
            {
                v = Constants._2PI - v;
            }

            v -= ArgumentOfPeriapsis();
            if (v < 0.0)
            {
                v += Constants._2PI;
            }

            return v % Constants._2PI;
        }
        
        private double CircularNoInclinationTrueAnomaly()
        {
            var l = System.Math.Acos(Position.X / Position.Magnitude());
            if (Velocity.X > 0)
            {
                l = Constants._2PI - l;
            }

            l = l - ArgumentOfPeriapsis() - AscendingNode();
            if (l < 0.0)
            {
                l += Constants._2PI;
            }

            return l % Constants._2PI;
        }

        public override double EccentricAnomaly()
        {
            double v = TrueAnomaly();
            double e = Eccentricity();
            return 2 * System.Math.Atan((System.Math.Tan(v / 2.0)) / System.Math.Sqrt((1 + e) / (1 - e)));
        }

        public override StateVector ToStateVector()
        {
            return this;
        }

        public override double MeanAnomaly()
        {
            return EccentricAnomaly() - Eccentricity() * System.Math.Sin(EccentricAnomaly());
        }

        public static StateVector operator +(StateVector sv1, StateVector sv2)
        {
            if (sv1.Epoch != sv2.Epoch || sv1.Frame != sv2.Frame)
            {
                throw new ArgumentException("State vector must have the same frame and the same epoch");
            }

            return new StateVector(sv1.Position + sv2.Position, sv1.Velocity + sv2.Velocity, sv2.Observer, sv1.Epoch, sv2.Frame);
        }

        public static StateVector operator -(StateVector sv1, StateVector sv2)
        {
            if (sv1.Epoch != sv2.Epoch || sv1.Frame != sv2.Frame)
            {
                throw new ArgumentException("State vector must have the same frame and the same epoch");
            }

            return new StateVector(sv1.Position - sv2.Position, sv1.Velocity - sv2.Velocity, sv2.Observer, sv1.Epoch, sv2.Frame);
        }


        public double[] ToArray()
        {
            return new[] { Position.X, Position.Y, Position.Z, Velocity.X, Velocity.Y, Velocity.Z };
        }

        public StateVector Inverse()
        {
            return new StateVector(Position.Inverse(), Velocity.Inverse(), Observer, Epoch, Frame);
        }


        public bool Equals(StateVector other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Position.Equals(other.Position) && Velocity.Equals(other.Velocity) && Observer.NaifId == other.Observer.NaifId && Epoch == other.Epoch && Frame == other.Frame;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((StateVector)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Position, Velocity);
        }

        public static bool operator ==(StateVector left, StateVector right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(StateVector left, StateVector right)
        {
            return !Equals(left, right);
        }
    }
}