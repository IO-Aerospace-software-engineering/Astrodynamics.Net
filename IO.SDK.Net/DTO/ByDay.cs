using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

public struct ByDay
{
    public int SiteId;
    public double TwilightDefinition;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1000)]
    public Window[] Windows;
}