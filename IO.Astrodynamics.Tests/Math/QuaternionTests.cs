using IO.Astrodynamics.Models.Body.Spacecraft;
using IO.Astrodynamics.Models.Math;
using IO.Astrodynamics.Models.Mission;
using Xunit;

namespace IO.Astrodynamics.Models.Tests.Math
{
    public class QuaternionTests
    {
        [Fact]
        public void CreateByAxisAngle()
        {
            Quaternion q = new Quaternion(Vector3.VectorZ, Constants.PI2);
            Assert.Equal(0.7071067811865476, q.W);
            Assert.Equal(0.0, q.VectorPart.X);
            Assert.Equal(0.0, q.VectorPart.Y);
            Assert.Equal(0.7071067811865475, q.VectorPart.Z, 12);
        }

        [Fact]
        public void Create()
        {
            Quaternion q = new Quaternion(1.0, 2.0, 3.0, 4.0);
            Assert.Equal(1.0, q.W);
            Assert.Equal(2.0, q.VectorPart.X);
            Assert.Equal(3.0, q.VectorPart.Y);
            Assert.Equal(4.0, q.VectorPart.Z);
        }

        [Fact]
        public void CreateByScalarVector()
        {
            Quaternion q = new Quaternion(1.0, new Vector3(2.0, 3.0, 4.0));
            Assert.Equal(1.0, q.W);
            Assert.Equal(2.0, q.VectorPart.X);
            Assert.Equal(3.0, q.VectorPart.Y);
            Assert.Equal(4.0, q.VectorPart.Z);
        }

        [Fact]
        public void Multiply()
        {
            Quaternion q1 = new Quaternion(Vector3.VectorX, 40.0 * Constants.Deg2Rad);
            Quaternion q2 = new Quaternion(Vector3.VectorY, 40.0 * Constants.Deg2Rad);
            Quaternion q3 = q1 * q2;
            Assert.Equal(0.8830222215594891, q3.W);
            Assert.Equal(0.3213938048432697, q3.VectorPart.X);
            Assert.Equal(0.3213938048432697, q3.VectorPart.Y);
            Assert.Equal(0.11697777844051097, q3.VectorPart.Z);
        }

        [Fact]
        public void Multiply2()
        {
            Quaternion q1 = new Quaternion(Vector3.VectorX.Inverse(), 40.0 * Constants.Deg2Rad);
            Quaternion q2 = new Quaternion(Vector3.VectorY, 40.0 * Constants.Deg2Rad);
            Quaternion q3 = q1 * q2;
            Assert.Equal(0.8830222215594891, q3.W);
            Assert.Equal(-0.3213938048432697, q3.VectorPart.X);
            Assert.Equal(0.3213938048432697, q3.VectorPart.Y);
            Assert.Equal(-0.11697777844051097, q3.VectorPart.Z);
        }

        [Fact]
        public void Multiply3()
        {
            Vector3 instrumentVector = Spacecraft.Front;

            Quaternion spacecraftOrientation = new Quaternion(Vector3.VectorZ, Constants.PI2);
            Quaternion instrumentOrientation = new Quaternion(Vector3.VectorZ, Constants.PI2);

            Quaternion globalOrientation = spacecraftOrientation * instrumentOrientation;

            Vector3 instrumentDirectionRelativeToWorld = instrumentVector.Rotate(globalOrientation);

            Assert.Equal(0.0, instrumentDirectionRelativeToWorld.X, 12);
            Assert.Equal(Vector3.VectorY.Inverse().Y, instrumentDirectionRelativeToWorld.Y, 12);
            Assert.Equal(0.0, instrumentDirectionRelativeToWorld.Z, 12);

        }

        [Fact]
        public void Multiply4()
        {
            Vector3 instrumentVector = Spacecraft.Front;

            Quaternion spacecraftOrientation = new Quaternion(Vector3.VectorZ, Constants.PI2);
            Quaternion instrumentOrientation = new Quaternion(Vector3.VectorX, Constants.PI2 * 0.5);

            Quaternion globalOrientation = spacecraftOrientation * instrumentOrientation;

            Vector3 instrumentDirectionRelativeToWorld = instrumentVector.Rotate(globalOrientation);

            Assert.Equal(-0.70710678118654757, instrumentDirectionRelativeToWorld.X, 12);
            Assert.Equal(0.0, instrumentDirectionRelativeToWorld.Y, 12);
            Assert.Equal(0.70710678118654757, instrumentDirectionRelativeToWorld.Z, 12);

        }

        [Fact]
        public void Magnitude()
        {
            Quaternion q1 = new Quaternion(Vector3.VectorX, 40.0 * Constants.Deg2Rad);
            Assert.Equal(1.0, q1.Magnitude());
        }

        [Fact]
        public void Normalize()
        {
            Quaternion q1 = new Quaternion(40.0 * Constants.Deg2Rad, new Vector3(2.0, 2.0, 2.0));
            Assert.Equal(3.533749831504592, q1.Magnitude());
            Quaternion q2 = q1.Normalize();
            Assert.Equal(0.19756115573707231, q2.W);
            Assert.Equal(0.56597102097305074, q2.VectorPart.X);
            Assert.Equal(0.56597102097305074, q2.VectorPart.Y);
            Assert.Equal(0.56597102097305074, q2.VectorPart.Z);
            Assert.Equal(1.0, q2.Magnitude());
        }

        [Fact]
        public void Conjugate()
        {
            Quaternion q1 = new Quaternion(40.0 * Constants.Deg2Rad, new Vector3(-2.0, 2.0, -2.0));
            Quaternion q2 = q1.Conjugate();
            Assert.Equal(40.0 * Constants.Deg2Rad, q2.W);
            Assert.Equal(2.0, q2.VectorPart.X);
            Assert.Equal(-2.0, q2.VectorPart.Y);
            Assert.Equal(2.0, q2.VectorPart.Z);
        }
    }
}
