using System.Globalization;
using Cocona;
using IO.Astrodynamics.Body;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.Frames;
using IO.Astrodynamics.Math;
using IO.Astrodynamics.OrbitalParameters;
using IO.Astrodynamics.Time;

namespace IO.Astrodynamics.CLI.Commands;

public class OrbitalParametersConverterCommand
{
    public OrbitalParametersConverterCommand()
    {
    }

    [Command("orbital-parameters-converter", Description = "Convert an orbital parameters to another type at given epoch in given frame")]
    public Task Converter(
        [Argument(Description = "Directory kernels path")]
        string kernelsPath,
        [Argument(Description = "Original orbital parameters. Elements must be separated by a coma")]
        string orbitalParametersInput,
        [Argument(Description = "Center of motion")]
        int centerofMotion,
        [Argument(Description = "Epoch")] string epoch,
        [Argument(Description = "Original frame")]
        string originalFrame,
        [Option('s', Description = "From state vector")]
        bool fromStateVector,
        [Option('k', Description = "From keplerian elements")]
        bool fromKeplerian,
        [Option('e', Description = "From equinoctial elements")]
        bool fromEquinoctial,
        [Option('t', Description = "From two lines elements")]
        bool fromTLE,
        [Option(Description = "Convert to state vector")]
        bool toStateVector,
        [Option(Description = "Convert to keplerian elements")]
        bool toKeplerian,
        [Option(Description = "Convert to Equinoctial")]
        bool toEquinoctial,
        [Option(Description = "Target epoch. If undefined original epoch will be used")] string targetEpoch = "",
        [Option(Description = "Target frame")] string targetFrame = "ICRF")
    {
        //Load kernels
        API.Instance.LoadKernels(new DirectoryInfo(kernelsPath));

        //Check inputs
        if (!(fromEquinoctial ^ fromKeplerian ^ fromStateVector ^ fromTLE))
        {
            throw new ArgumentException("You must set the original orbital parameters type. use --help for more information");
        }

        if (!(toStateVector ^ toKeplerian ^ toEquinoctial))
        {
            throw new ArgumentException("You must set the target orbital parameters type. use --help for more information");
        }
        
        //Clean inputs
        if (originalFrame.Equals("icrf", StringComparison.InvariantCultureIgnoreCase))
        {
            originalFrame = "j2000";
        }
        if (targetFrame.Equals("icrf", StringComparison.InvariantCultureIgnoreCase))
        {
            targetFrame = "j2000";
        }

        if (string.IsNullOrEmpty(targetEpoch))
        {
            targetEpoch = epoch;
        }

        //Initialize data
        var inputFrame = new Frame(originalFrame);
        var outputFrame = new Frame(targetFrame);
        var inputEpoch = Helpers.ConvertDateTimeInput(epoch);
        var outputEpoch = Helpers.ConvertDateTimeInput(targetEpoch);
        var inputCenterOfMotion = new CelestialBody(centerofMotion);

        //Generate original orbital parameters
        OrbitalParameters.OrbitalParameters orbitalParameters = null;
        if (fromStateVector)
        {
            var arr = orbitalParametersInput.Split('\n').Select(double.Parse).ToArray();
            orbitalParameters = new StateVector(new Vector3(arr[0], arr[1], arr[2]), new Vector3(arr[3], arr[4], arr[5]), inputCenterOfMotion, inputEpoch, inputFrame);
        }
        else if (fromKeplerian)
        {
            var arr = orbitalParametersInput.Split('\n').Select(double.Parse).ToArray();
            orbitalParameters = new KeplerianElements(arr[0], arr[1], arr[2], arr[3], arr[4], arr[5], inputCenterOfMotion, inputEpoch, inputFrame);
        }
        else if (fromEquinoctial)
        {
            var arr = orbitalParametersInput.Split('\n').Select(double.Parse).ToArray();
            orbitalParameters = new EquinoctialElements(arr[0], arr[1], arr[2], arr[3], arr[4], arr[5], inputCenterOfMotion, inputEpoch, inputFrame);
        }
        else if (fromTLE)
        {
            var arr = orbitalParametersInput.Split(',').ToArray();
            orbitalParameters = TLE.Create("body", arr[0], arr[1]);
        }

        //At given epoch and in given frame
        orbitalParameters = orbitalParameters!.AtEpoch(outputEpoch).ToFrame(outputFrame);

        //Generate required output
        if (toStateVector)
        {
            orbitalParameters = orbitalParameters.ToStateVector();
        }
        else if (toKeplerian)
        {
            orbitalParameters = orbitalParameters.ToKeplerianElements();
        }
        else if (toEquinoctial)
        {
            orbitalParameters = orbitalParameters.ToEquinoctial();
        }

        Console.WriteLine(orbitalParameters.ToString());

        return Task.CompletedTask;
    }
}