using System.Runtime.InteropServices;
using IO.SDK.Net.DTO;
using Xunit;

namespace IO.SDK.Net.Tests;

public class APITest
{
    [Fact]
    public void CheckVersion()
    {
        API api = new API();
        Assert.Equal("CSPICE_N0067", api.GetSpiceVersion());
    }

    [Fact]
    public void Execute()
    {
        API api = new API();
        var scenario = new Scenario();
        scenario.Name = "titi";
        scenario.Window.Start = 10.0;
        scenario.Window.End = 20.0;
        var res = api.ExecuteScenario(scenario);
        Assert.Equal("titi", res.Name);
        Assert.Equal(10.0, res.Window.Start);
        Assert.Equal(20.0, res.Window.End);
    }
    
    [Fact]
    public void CheckSize()
    {
        var scenario = new Scenario();
        var size = Marshal.SizeOf(scenario);
        Assert.Equal(2160664,size);
    }
}