using System.ComponentModel;

namespace IO.SDK.Net;

public enum OccultationType
{
    [Description("FULL")]
    Full,
    [Description("ANNULAR")]
    Annular,
    [Description("PARTIAL")]
    Partial,
    [Description("ANY")]
    Any,
}