namespace IO.SDK.Net.DTO;

public struct Instrument
{
    public int Id;
    public string Shape;
    public Vector3D Orientation;
    public Vector3D Boresight;
    public Vector3D FovRefVector;
    public double FieldOfView;
}