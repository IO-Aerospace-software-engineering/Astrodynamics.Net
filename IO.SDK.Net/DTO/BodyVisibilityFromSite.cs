using System.Runtime.InteropServices;

namespace IO.SDK.Net.DTO;

public struct BodyVisibilityFromSite
{ 
    public int SiteId;
    public int TargetBodyId;
    public string Aberration;
    
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1000)]
    public Window[] Windows;
    
}