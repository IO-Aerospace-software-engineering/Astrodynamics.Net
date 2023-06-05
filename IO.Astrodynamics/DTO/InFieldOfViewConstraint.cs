// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System.Runtime.InteropServices;

namespace IO.Astrodynamics.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct InFieldOfViewConstraint
{
    public int TargetId;
    public string Aberration;
    public double InitialStepSize;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1000)]
    public Window[] Windows;

    public InFieldOfViewConstraint(int targetId, string aberration, double initialStepSize) : this()
    {
        TargetId = targetId;
        Aberration = aberration;
        InitialStepSize = initialStepSize;
        Windows = new Window[1000];
    }
}