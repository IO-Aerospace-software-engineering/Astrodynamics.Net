// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System.Runtime.InteropServices;

namespace IO.Astrodynamics.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct CelestialBody
{
    public int Id;
    public int CenterOfMotionId;
    public string Name;
    public Vector3D Radii;
    public double GM;
    public string FrameName;
    public int FrameId;
    public string Error;

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