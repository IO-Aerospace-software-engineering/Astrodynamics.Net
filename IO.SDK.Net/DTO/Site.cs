using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct Site
{
    private const int AZIMUTHRANGESIZE = 10;
    public int Id=0;
    public string Name = null;
    public int BodyId = 0;
    
    public Geodetic Coordinates = default;
    
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = AZIMUTHRANGESIZE)]
    public AzimuthRange[] Ranges = new AzimuthRange[AZIMUTHRANGESIZE];

    public string DirectoryPath = null;

    public Site()
    {
    }

    // [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    // public BodyVisibilityFromSite[] BodyVisibilityFromSites;
    //
    // public ByDay ByDay;
    //
    // public ByNight ByNight;
}