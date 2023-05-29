using System;
using System.ComponentModel;
using System.Reflection;

namespace IO.SDK.Net
{

    public enum Aberration
    {
        [Description("NONE")] None,
        [Description("LT")] LT,
        [Description("LT+S")] LTS,
        [Description("CN")] CN,
        [Description("CN+S")] CNS,
        [Description("XLT")] XLT,
        [Description("XLT+S")] XLTS,
        [Description("XCN")] XCN,
        [Description("XCN+S")] XCNS

    }
}