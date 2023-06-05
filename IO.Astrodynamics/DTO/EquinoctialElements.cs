// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System.Runtime.InteropServices;

namespace IO.Astrodynamics.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct EquinoctialElements
{
    public double Epoch;
    public int CenterOfMotionId;
    public string Frame;
    public double SemiMajorAxis;
    public double H;
    public double K;
    public double P;
    public double Q;
    public double L;
    public double PeriapsisLongitudeRate;
    public double RightAscensionOfThePole;
    public double DeclinationOfThePole;
    public double AscendingNodeLongitudeRate;
    public double Period;
}