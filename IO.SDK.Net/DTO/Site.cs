using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

public struct Site
{
    int Id;
    string Name;
    int BodyId;
    
    Geodetic Coordinates;
    
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    AzimuthRange[] Ranges;
}