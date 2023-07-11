// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System.Runtime.InteropServices;

namespace IO.Astrodynamics.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct ConicElements
{
    public readonly int CenterOfMotionId;
    public readonly double Epoch;
    public readonly double PerifocalDistance;
    public readonly double Eccentricity;
    public readonly double Inclination;
    public readonly double AscendingNodeLongitude;
    public readonly double PeriapsisArgument;
    public readonly double MeanAnomaly;
    public readonly double TrueAnomaly;
    public readonly double OrbitalPeriod;
    public readonly double SemiMajorAxis;
    public readonly string Frame;
}