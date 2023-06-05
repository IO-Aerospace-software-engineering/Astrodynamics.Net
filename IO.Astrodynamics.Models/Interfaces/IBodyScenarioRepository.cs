using System.Threading.Tasks;
using IO.Astrodynamics.Models.Mission;
using IO.Astrodynamics.Models.SeedWork;

namespace IO.Astrodynamics.Models.Interfaces
{
    public interface IBodyScenarioRepository : IRepository<BodyScenario>
    {
        public Task<BodyScenario> GetFromBodyAsync(int bodyId, int scenarioId);
        public Task<BodyScenario> GetFromBodyScenarioAsync(int bodyScenarioId);
    }
}