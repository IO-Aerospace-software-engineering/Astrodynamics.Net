using System.ComponentModel;

namespace IO.SDK.Net;

public enum RelationnalOperator
{
    [Description(">")]
    Greater,
    [Description("<")]
    Lower,
    [Description("=")]
    Equal,
    [Description("ABSMIN")]
    AbsoluteMin,
    [Description("ABSMAX")]
    AbsoluteMax,
    [Description("LOCMIN")]
    LocalMin,
    [Description("LOCMAX")]
    LocalMax,
}