using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct DistanceConstraint
{
    public int Observerid;
    public int TargetId;
    public string Constraint;
    public double Value;
    public string Aberration;
    public double InitialStepSize;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1000)]
    public Window[] Windows;

    public DistanceConstraint(int observerId, int targetId, string constraint, double value, string aberration,
        double initialStepSize)
    {
        Observerid = observerId;
        TargetId = targetId;
        Constraint = constraint;
        Value = value;
        Aberration = aberration;
        InitialStepSize = initialStepSize;
        Windows = new Window[1000];
    }
}