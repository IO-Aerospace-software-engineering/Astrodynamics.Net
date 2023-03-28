using Xunit;

namespace IO.SDK.Net.Tests;

public class APITest
{
    [Fact]
    public void CheckVersion()
    {
        IO.SDK.Net.API api = new API();
        Assert.Equal("CSPICE_N0067", api.GetSpiceVersion());
    }
}