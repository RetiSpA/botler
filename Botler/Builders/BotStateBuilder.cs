using System.Threading.Tasks;
using Botler.Dialogs.Scenari;
using Botler.Models;
using Botler.Middleware.Services;
using Microsoft.Bot.Builder;
using static Botler.Dialogs.Utility.Commands;
using static Botler.Dialogs.Utility.Scenari;
namespace Botler.Builders
{
    public static class BotStateBuilder
    {
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

        public async static Task<BotStateContext> BuildAndSaveBotStateContextContext(ITurnContext turn, BotlerAccessors accessors, LuisServiceResult luisServiceResult, string scenarioID, Intent intent)
        {
            BotStateContext state = new BotStateContext();
            var score = luisServiceResult.TopScoringIntent.Item2;

            state.Turn = await accessors.GetCurretTurnCounterAsync(turn);
            state.UserQuery = turn.Activity.Text;
            state.Conversation_ID = turn.Activity.From.Id;
            state.scenarioID = scenarioID;

            if (intent is null)
            {
                state.TopIntent = IntentFactory.FactoryMethod(luisServiceResult);
            }
            else
            {
                state.TopIntent = intent;
                state.TopIntent.EntitiesCollected = luisServiceResult.AllEntitiesFromLuis;
            }
            await accessors.MongoDB.InsertJSONContextDocAsync(state);

            return state;
        }
    }
}