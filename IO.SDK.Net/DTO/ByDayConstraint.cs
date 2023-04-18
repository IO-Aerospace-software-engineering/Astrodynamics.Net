using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct ByDayConstraint
{
    public double TwilightDefinition;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1000)]
    public Window[] Windows;

    public ByDayConstraint(double twilightDefinition) : this()
    {
        Windows = new Window[1000];
        TwilightDefinition = twilightDefinition;
    }
}