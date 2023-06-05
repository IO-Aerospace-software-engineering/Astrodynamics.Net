// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct StateOrientation
{
    public Quaternion Orientation;
    public Vector3D AngularVelocity;
    public double Epoch;
    public string Frame;

    public StateOrientation(Quaternion orientation, Vector3D angularVelocity, double epoch, string frame)
    {
        Orientation = orientation;
        AngularVelocity = angularVelocity;
        Epoch = epoch;
        Frame = frame;
    }
}