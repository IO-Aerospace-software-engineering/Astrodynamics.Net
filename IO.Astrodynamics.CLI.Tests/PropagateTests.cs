// Copyright 2024. Sylvain Guillet (sylvain.guillet@tutamail.com)

using System.Globalization;
using System.Text;
using IO.Astrodynamics.CLI.Commands;
using IO.Astrodynamics.CLI.Commands.Parameters;

namespace IO.Astrodynamics.CLI.Tests;

public class PropagateTests
{
    [Fact]
    public void PropagateWithPerturbations()
    {
        lock (Configuration.objLock)
        {
            var command = new PropagateCommand();
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            Console.SetOut(sw);
            command.Propagate("Data", -180,
                new Commands.Parameters.OrbitalParameters
                {
                    CenterOfMotionId = 399, EpochParameter = new EpochParameters { Epoch = "0.0" }, Frame = "ICRF", OrbitalParametersValues = "6800000.0 0.0 0.0 0.0 8000.0 0.0",
                    FromStateVector = true
                }, new WindowParameters { Begin = new EpochParameters { Epoch = "0.0" }, End = new EpochParameters { Epoch = "3600.0" } }, [10, 301], "PropagatorExport", true,
                true, 20).Wait();

            var res = sb.ToString();
            Assert.Equal($"Propagation completed. You can use generated kernels here /home/spacer/Sources/SDK.Net/IO.Astrodynamics.CLI.Tests/bin/Debug/net8.0/PropagatorExport or visualize this directory into Cosmographia\n", res);
        }
    }
}