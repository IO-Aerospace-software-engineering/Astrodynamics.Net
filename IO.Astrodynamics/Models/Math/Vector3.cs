using System.Text.Json.Serialization;

namespace IO.Astrodynamics.Models.Math
{
    public readonly record struct Vector3
    {
        public double X { get; }
        public double Y { get; }
        public double Z { get; }

        public static Vector3 VectorX { get; } = new Vector3(1.0, 0.0, 0.0);
        public static Vector3 VectorY { get; } = new Vector3(0.0, 1.0, 0.0);
        public static Vector3 VectorZ { get; } = new Vector3(0.0, 0.0, 1.0);
        public static Vector3 Zero { get; private set; } = new Vector3(0.0, 0.0, 0.0);

        [JsonConstructor]
        public Vector3(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double Magnitude()
        {
            return System.Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        public Vector3 Normalize()
        {
            return this / Magnitude();
        }

        public Vector3 Cross(Vector3 vector)
        {
            return new Vector3(Y * vector.Z - Z * vector.Y, Z * vector.X - X * vector.Z, X * vector.Y - Y * vector.X);
        }

        public double Angle(Vector3 vector)
        {
            return System.Math.Acos(this * vector / (Magnitude() * vector.Magnitude()));
        }

        public Vector3 Inverse()
        {
            return this * -1.0;
        }

        public static Vector3 operator *(Vector3 v, double value)
        {
            return new Vector3(v.X * value, v.Y * value, v.Z * value);
        }

        public static double operator *(Vector3 v, Vector3 value)
        {
            return v.X * value.X + v.Y * value.Y + v.Z * value.Z;
        }

        public static Vector3 operator /(Vector3 v, double value)
        {
            return new Vector3(v.X / value, v.Y / value, v.Z / value);
        }

        public static Vector3 operator +(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
        }

        public static Vector3 operator -(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
        }

        public Quaternion To(Vector3 vector)
        {
            var dot = this * vector;

            if (dot == -1.0)//Manage 180° case
            {
                double x = System.Math.Abs(vector.X);
                double y = System.Math.Abs(vector.Y);
                double z = System.Math.Abs(vector.Z);

                Vector3 axis = x < y ? (x < z ? VectorX : VectorZ) : (y < z ? VectorY : VectorZ);
                var vec = vector.Cross(axis);
                return new Quaternion(0.0, vec.X, vec.Y, vec.Z).Normalize();
            }

            var mag1 = Magnitude();
            var mag2 = vector.Magnitude();
            var v = vector.Cross(this);
            var w = dot + System.Math.Sqrt(mag1 * mag1 * mag2 * mag2);

            return new Quaternion(w, v.X, v.Y, v.Z).Normalize();
        }

        public Vector3 Rotate(Quaternion quaternion)
        {
            var p = new Quaternion(0.0, this);
            return (quaternion * p * quaternion.Conjugate()).VectorPart;
        }

        public override string ToString()
        {
            return $"X : {X} - Y : {Y} - Z : {Z}";
        }
    }
}