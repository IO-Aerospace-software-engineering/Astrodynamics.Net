using System;
using System.Collections.Generic;
using IO.Astrodynamics.Models.Body;
using IO.Astrodynamics.Models.Frames;
using IO.Astrodynamics.Models.Math;

namespace IO.Astrodynamics.Models.OrbitalParameters
{
    public class StateVector : OrbitalParameters, IEquatable<StateVector>
    {
        public Vector3 Position { get; set; }
        public Vector3 Velocity { get; set; }

        StateVector()
        { }
        public StateVector(Vector3 position, Vector3 velocity, CelestialBody centerOfMotion, DateTime epoch, Frame frame) : base(centerOfMotion, epoch, frame)
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
            return (Velocity.Cross(SpecificAngularMomentum()) / CenterOfMotion.GM) - (Position / Position.Magnitude());
        }

        public override double Inclination()
        {
            return SpecificAngularMomentum().Angle(Vector3.VectorZ);
        }


        public override double SpecificOrbitalEnergy()
        {
            return System.Math.Pow(Velocity.Magnitude(), 2.0) / 2.0 - (CenterOfMotion.GM / Position.Magnitude());
        }

        public override double SemiMajorAxis()
        {
            return -(CenterOfMotion.GM / (2.0 * SpecificOrbitalEnergy()));
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
            var e = EccentricityVector();
            var v = System.Math.Acos((e * Position) / (e.Magnitude() * Position.Magnitude()));
            if (Position * Velocity < 0.0)
            {
                v = System.Math.PI * 2.0 - v;
            }

            return v;
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
            return new StateVector(sv1.Position + sv2.Position, sv1.Velocity + sv2.Velocity, sv2.CenterOfMotion, sv1.Epoch, sv2.Frame);
        }

        public static StateVector operator -(StateVector sv1, StateVector sv2)
        {
            if (sv1.Epoch != sv2.Epoch || sv1.Frame != sv2.Frame)
            {
                throw new ArgumentException("State vector must have the same frame and the same epoch");
            }
            return new StateVector(sv1.Position - sv2.Position, sv1.Velocity - sv2.Velocity, sv2.CenterOfMotion, sv1.Epoch, sv2.Frame);
        }

        public static bool operator ==(StateVector left, StateVector right)
        {
            return EqualityComparer<StateVector>.Default.Equals(left, right);
        }

        public static bool operator !=(StateVector left, StateVector right)
        {
            return !(left == right);
        }

        public double[] ToArray()
        {
            return new[] { Position.X, Position.Y, Position.Z, Velocity.X, Velocity.Y, Velocity.Z };
        }

        public StateVector Inverse()
        {
            return new StateVector(Position.Inverse(), Velocity.Inverse(), CenterOfMotion, Epoch, Frame);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as StateVector);
        }

        public bool Equals(StateVector other)
        {
            return other is not null &&
                   base.Equals(other) &&
                   Position.Equals(other.Position) &&
                   Velocity.Equals(other.Velocity);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Position, Velocity);
        }
    }
}