namespace IO.SDK.Net.DTO;

public struct StateOrientation
{
    public Quaternion Orientation;
    public Vector3D AngularVelocity;
    public double Epoch;
    public string Frame;
}