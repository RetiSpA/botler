using System.Threading.Tasks;
using Botler.Dialogs.Scenari;
using Botler.Models;
using Botler.Middleware.Services;
using Botler.Builders.IntentBuilders;
using Microsoft.Bot.Builder;
using Newtonsoft.Json;
using System;
using static Botler.Dialogs.Utility.Commands;
using static Botler.Dialogs.Utility.Scenari;
using Botler.Helpers;

namespace Botler.Builders
{
    /// <summary>
    /// This class Build the BotStateContext Model, that we'll use to understand the actual
    /// conversation's context
    /// For each ConversationID -> Unique BotStateContext entity
    /// </summary>
    public static class BotStateBuilder
    {
        /// <summary>
        /// Build the first turn BotStateContext and save it in the CosmoDB
        /// </summary>
        /// <param name="accessors"></param>
        /// <param name="turn"></param>
        /// <returns></returns>
        public async static Task<BotStateContext> BuildFirstTurnBotStateContext(BotlerAccessors accessors, ITurnContext turn)
        {
            BotStateContext state = new BotStateContext();
            state.Turn = 0;
            state.UserQuery = turn.Activity.Text;
            state.Conversation_ID = turn.Activity.From.Id;
            state.scenarioID = Default;
            await accessors.SaveLastBotStateContext(turn, state);
            return state;
        }

        /// <summary>
        /// Build and save a generic botstate context based on this turn.
        /// </summary>
        /// <param name="turn">Current bot Turn</param>
        /// <param name="accessors">MemoryStorage manager</param>
        /// <param name="luisServiceResult">LuisResult for this turn</param>
        /// <param name="scenarioID">Scenario ID </param>
        /// <param name="intent">Builded TopIntent from LUIS</param>
        /// <returns></returns>

        public async static Task<BotStateContext> BuildAndSaveBotStateContextContext(ITurnContext turn, BotlerAccessors accessors, LuisServiceResult luisServiceResult, IScenario scenario)
        {
            BotStateContext state = new BotStateContext();
            var score = luisServiceResult.TopScoringIntent.Item2;

            state.Turn = await accessors.GetCurretTurnCounterAsync(turn);
            state.UserQuery = turn.Activity.Text;
            state.Conversation_ID = turn.Activity.From.Id;
            state.scenarioID = scenario.ScenarioID;

            if (scenario.ScenarioIntent is null)
            {
                state.TopIntent = IntentFactory.FactoryMethod(luisServiceResult);
                state.TopIntent.EntitiesCollected = luisServiceResult.AllEntitiesFromLuis;
            }
            else
            {
                state.TopIntent = scenario.ScenarioIntent;
                state.TopIntent.EntitiesCollected = scenario.ScenarioIntent.EntitiesCollected;
            }

            await accessors.MongoDB.InsertJSONContextDocAsync(state);
            var json = JsonConvert.SerializeObject(state);
            return state;
        }
    }
}