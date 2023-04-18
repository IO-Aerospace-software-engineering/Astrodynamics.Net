using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct Instrument
{
    public int Id;
    public string Name;
    public string Shape;
    public Vector3D Orientation;
    public Vector3D Boresight;
    public Vector3D FovRefVector;
    public double FieldOfView;
    public double CrossAngle;
    // [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    // public InFieldOfView[] InFieldOfViews;
    public Instrument(int id, string name, string shape, Vector3D orientation, Vector3D boresight, Vector3D fovRefVector, double fieldOfView, double crossAngle)
    {
        Id = id;
        Name = name;
        Shape = shape;
        Orientation = orientation;
        Boresight = boresight;
        FovRefVector = fovRefVector;
        FieldOfView = fieldOfView;
        CrossAngle = crossAngle;
    }
}