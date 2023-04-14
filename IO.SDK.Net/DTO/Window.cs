using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct Window
{
    public double Start, End;

    public Window(double start, double end)
    {
        Start = start;
        End = end;
    }
}