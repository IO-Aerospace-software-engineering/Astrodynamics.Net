using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct Site
{
    int Id;
    string Name;
    int BodyId;
    
    Geodetic Coordinates;
    
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    AzimuthRange[] Ranges;
    
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public BodyVisibilityFromSite[] BodyVisibilityFromSites;
    
    public ByDay ByDay;
    
    public ByNight ByNight;
}