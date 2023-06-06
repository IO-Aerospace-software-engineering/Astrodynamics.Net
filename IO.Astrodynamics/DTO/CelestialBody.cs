// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System.Runtime.InteropServices;

namespace IO.Astrodynamics.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct CelestialBody
{
    public int Id { get; }
    public int CenterOfMotionId { get; }
    public string Name { get; }
    public Vector3D Radii { get; }
    public double GM { get; }
    public string FrameName { get; }
    public int FrameId { get; }
    public string Error { get; }

    public CelestialBody(int id, int centerOfMotionId, string name, Vector3D radii, double gm, string frameName,
        int frameId, string error)
    {
        Id = id;
        CenterOfMotionId = centerOfMotionId;
        Name = name;
        Radii = radii;
        GM = gm;
        FrameName = frameName;
        FrameId = frameId;
        Error = error;
    }
}