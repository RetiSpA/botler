using System.Threading.Tasks;
using Botler.Controllers;
using Botler.Models;
using Botler.Middleware.Services;
using Botler.Builders;
using Microsoft.Bot.Builder;



namespace Botler.Dialogs.Scenari
{
    public abstract class ExecutionScenario : IScenario
    {
        public abstract Intent ScenarioIntent { get; set; }

        public abstract bool NeedAuthentication { get; set; }

        public abstract string ScenarioID { get; set; }

        public abstract string AssociatedScenario { get; set; }

        public abstract Task CreateResponseAsync(LuisServiceResult luisServiceResult);

        public async Task HandleScenarioStateAsync(ITurnContext turn, BotlerAccessors accessors, LuisServiceResult luisServiceResult)
        {
            await CreateResponseAsync(luisServiceResult);
        }
    }

}