using Cocona;

namespace IO.Astrodynamics.CLI.Commands;

public class EphemerisCommand
{
    public EphemerisCommand()
    {
    }

    [Cocona.Command("ephemeris", Description = "Compute ephemeris of given body")]
    public Task Ephemeris([Argument(Description = "The name to display")] string name)
    {
        return Task.FromResult(() => Console.WriteLine($"Name : {name}"));
    }
}