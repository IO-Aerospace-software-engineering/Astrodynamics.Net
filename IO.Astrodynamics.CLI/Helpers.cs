using IO.Astrodynamics.Body;
using IO.Astrodynamics.Body.Spacecraft;
using IO.Astrodynamics.Surface;

namespace IO.Astrodynamics.CLI;

public class Helpers
{
    internal static ILocalizable CreateLocalizable(int objectId)
    {
        ILocalizable localizableObject = null;
        if (int.IsPositive(objectId))
        {
            if (objectId is > 100_000 and < 999_999)
            {
                var id = int.Parse(objectId.ToString().Skip(3).ToString()!);
                localizableObject = new Site(id, $"site{id}", new CelestialBody((int)double.Truncate(objectId * 1E-03)));
            }

            localizableObject ??= new CelestialBody(objectId);
        }
        else
        {
            localizableObject = new Spacecraft(objectId, $"spc{objectId}", 1.0, 1.0, new Clock($"clk{objectId}", 1 / 65536.0), null);
        }

        return localizableObject;
    }
    
    internal static IOrientable CreateOrientable(int objectId)
    {
        IOrientable celestialItem;
        if (int.IsPositive(objectId))
        {
            celestialItem = new CelestialBody(objectId);
        }
        else
        {
            celestialItem = new Spacecraft(objectId, "spc", 1.0, 1.0, new Clock("clk", 1 / 65536.0), null);
        }

        return celestialItem;
    }
}