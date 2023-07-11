// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System.Runtime.InteropServices;

namespace IO.Astrodynamics.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct EquinoctialElements
{
    public readonly double Epoch;
    public readonly int CenterOfMotionId;
    public readonly string Frame;
    public readonly double SemiMajorAxis;
    public readonly double H;
    public readonly double K;
    public readonly double P;
    public readonly double Q;
    public readonly double L;
    public readonly double PeriapsisLongitudeRate;
    public readonly double RightAscensionOfThePole;
    public readonly double DeclinationOfThePole;
    public readonly double AscendingNodeLongitudeRate;
    public readonly double Period;
}