using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct Occultation
{
    public int ObserverId;
    public int BackBodyId;
    public int FrontId;
    public string Type;
    public string AberrationId;
    public double InitialStepSize;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1000)]
    public Window[] Windows;
}