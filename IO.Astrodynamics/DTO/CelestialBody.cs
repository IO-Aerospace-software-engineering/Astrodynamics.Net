// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System.Runtime.InteropServices;

namespace IO.Astrodynamics.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public readonly struct CelestialBody
{
    public int Id { get; }
    public int CenterOfMotionId { get; }
    public int BarycenterOfMotionId { get; }
    public string Name { get; }
    public Vector3D Radii { get; }
    public double GM { get; }
    public string FrameName { get; }
    public int FrameId { get; }
    public string Error { get; } = string.Empty;
    public double J2 { get; }
    public double J3 { get; }
    public double J4 { get; }

    public CelestialBody(int id, int centerOfMotionId, int barycenterOfMotionId, string name, Vector3D radii, double gm, string frameName, int frameId, double j2, double j3, double j4)
    {
        Id = id;
        CenterOfMotionId = centerOfMotionId;
        Name = name;
        Radii = radii;
        GM = gm;
        FrameName = frameName;
        FrameId = frameId;
        J2 = j2;
        J3 = j3;
        J4 = j4;
        BarycenterOfMotionId = barycenterOfMotionId;
    }

    public bool HasError()
    {
        return !string.IsNullOrEmpty(Error);
    }
}