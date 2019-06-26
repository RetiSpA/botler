using System;
using System.Threading.Tasks;
using Botler.Builders;
using Botler.Controllers;
using Botler.Models;
using Botler.Middleware.Services;
using Botler.Builders.IntentBuilders;
using Microsoft.Bot.Builder;

namespace Botler.Dialogs.Scenari
{
    public abstract class DescriptionScenario : IScenario
    {
        public abstract bool NeedAuthentication { get; set; }
        public abstract string ScenarioID { get; set; }
        public abstract Intent ScenarioIntent { get; set; }
        public abstract string AssociatedScenario { get; set; }

        public bool EntityLowerBoundReach()
        {
            return (ScenarioIntent.EntitiesCollected.Count >= ScenarioIntent.EntityLowerBound);
        }

        public async Task HandleScenarioStateAsync(ITurnContext turn, BotlerAccessors accessors, LuisServiceResult luisServiceResult)
        {
            SaveEntities(luisServiceResult);
            // * Chiedi più informazioni e//
            await turn.SendActivityAsync(ScenarioIntent.EntityNeedResponse);
        }

        public void SaveEntities(LuisServiceResult luisServiceResult)
        {
            if(ScenarioIntent is null)
            {
                ScenarioIntent = IntentFactory.FactoryMethod(luisServiceResult);
            }

            foreach(var e in luisServiceResult.AllEntitiesFromLuis)
            {
                if (!ScenarioIntent.EntitiesCollected.Contains(e))
                {
                    ScenarioIntent.EntitiesCollected.Add(e);
                }
            }

        }

    }
}