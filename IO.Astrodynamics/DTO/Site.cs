// Copyright 2023. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System.Runtime.InteropServices;

namespace IO.Astrodynamics.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct Site
{
    private const int AZIMUTH_RANGE_SIZE = 10;
    public int Id = 0;
    public string Name = null;
    public int BodyId = -1;

    public Planetodetic Coordinates = default;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = AZIMUTH_RANGE_SIZE)]
    public AzimuthRange[] Ranges;

    public string DirectoryPath = null;
    public string Error { get; } = string.Empty;


    public Site(int naifId, int bodyId, Planetodetic coordinates, string name, string directoryPath) : this()
    {
        Id = naifId;
        BodyId = bodyId;
        Coordinates = coordinates;
        Name = name;
        DirectoryPath = directoryPath;
        Ranges = ArrayBuilder.ArrayOf<AzimuthRange>(AZIMUTH_RANGE_SIZE);
    }
    
    public bool HasError()
    {
        return !string.IsNullOrEmpty(Error);
    }
}