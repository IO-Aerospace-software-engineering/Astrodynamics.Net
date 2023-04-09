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
}