namespace IO.Astrodynamics.Math;

public class Plane
{
    public Vector3 Normal { get; }
    public double Distance { get; }

    public Plane(Vector3 normal) : this(normal, 0.0)
    {
    }

    public Plane(Vector3 normal, double distance)
    {
        Normal = normal;
        Distance = distance;
    }

    public double GetAngle(Plane plane)
    {
        return System.Math.Acos((Normal * plane.Normal) / (Normal.Magnitude() * plane.Normal.Magnitude()));
    }

    public double GetAngle(Vector3 vector)
    {
        return System.Math.Asin((Normal * vector) / (Normal.Magnitude() * vector.Magnitude()));
    }
}