using System.Net.Http;

namespace IO.SDK.Net.DTO;

public struct StateVector
{
    public double Epoch;
    public Vector3D Position;
    public Vector3D Velocity;
    public CelestialBody CenterOfMotion;
}