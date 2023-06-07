// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System.Runtime.InteropServices;

namespace IO.Astrodynamics.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct Site
{
    private const int AZIMUTHRANGESIZE = 10;
    public int Id = 0;
    public string Name = null;
    public int BodyId = -1;

    public Geodetic Coordinates = default;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = AZIMUTHRANGESIZE)]
    public AzimuthRange[] Ranges;

    public string DirectoryPath = null;


    //Todo check if id must be (ex. 13 or 399013) and body id is necessary
    public Site(int id, int bodyId, Geodetic coordinates, string name, string directoryPath) : this()
    {
        Id = id;
        BodyId = bodyId;
        Coordinates = coordinates;
        Name = name;
        DirectoryPath = directoryPath;
        Ranges = ArrayBuilder.ArrayOf<AzimuthRange>(AZIMUTHRANGESIZE);
    }

    // [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    // public BodyVisibilityFromSite[] BodyVisibilityFromSites;
    //
    // public ByDay ByDay;
    //
    // public ByNight ByNight;
}