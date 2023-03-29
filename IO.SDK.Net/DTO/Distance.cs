using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct Distance
{
    public int Observerid;
    public int TargetId;
    public string Constraint;
    public double Value;
    public string Aberration;
    public double InitialStepSize;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
    public Window[] Windows;
}