using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

public struct InFieldOfView
{
    public int InstrumentId;
    public int TargetId;
    public string Aberration;
    public double InitialStepSize;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1000)]
    public Window[] Windows;
}