
using Botler.Dialogs.Scenari;
using Botler.Middleware.Services;
using Botler.Models;
using Botler.Controllers;
using Botler.Builders;
using Microsoft.Bot.Builder;
using System.Threading.Tasks;
using static Botler.Dialogs.Utility.LuisIntent;
using static Botler.Dialogs.Utility.ListsResponsesIT;
using static Botler.Dialogs.Utility.Responses;
using static Botler.Dialogs.Utility.Scenari;
using static Botler.Dialogs.Utility.IntentsSets;
using Botler.Commands;
using Botler.Helpers;

namespace Botler.Controller
{
    /// <summary>
    /// This is class is useful to return the actual turn's scenario, with luis intents (LEGGE ed INTERPRETA IL TURNO ATTUALE)
    /// </summary>
    public static class ScenarioRecognizer
    {
        /// <summary>
        /// With LUIS intents finds the most accurate turn's scenario
        /// </summary>
        /// <param name="luisServiceResult">LuisResult from this turn</param>
        /// <param name="accessors"></param>
        /// <param name="turn">Currenct instance of ITurnContext</param>
        /// <returns></returns>
        public static async Task<IScenario> ExtractCurrentScenarioAsync(LuisServiceResult luisServiceResult, BotlerAccessors accessors, ITurnContext turn)
        {
            IScenario topIntentScenario = GetScenarioFromLuis(luisServiceResult, accessors, turn);

            // Reads the conversation history to find the last useful scenario and understant the context
            if (topIntentScenario.ScenarioID.Equals(Default))
            {
                topIntentScenario = await BotStateContextController.CheckBotState(topIntentScenario, accessors, turn, luisServiceResult);
            }

            if (topIntentScenario.NeedAuthentication)
            {
                var alreadyAuth = await AuthenticationHelper.UserAlreadyAuthAsync(turn, accessors);

                if (alreadyAuth)
                {
                    return topIntentScenario;
                }
                else
                {
                    // await turn.SendActivityAsync(RandomResponses(AutenticazioneNecessariaResponse));
                    topIntentScenario = ScenarioFactory.FactoryMethod(accessors, turn, Autenticazione, null);
                }
            }

            return topIntentScenario;
        }

        private static IScenario GetScenarioFromLuis(LuisServiceResult luisServiceResult, BotlerAccessors accessors, ITurnContext turn)
        {
            Intent intent = luisServiceResult.TopIntent;

            if (isAParkingIntent(luisServiceResult))
            {
                return ScenarioFactory.FactoryMethod(accessors, turn, Parking, intent);
            }

            if (isAOutlookIntent(luisServiceResult))
            {
                if (intent.EntitiesCollected.Count < intent.EntityLowerBound)
                {
                  return ScenarioFactory.FactoryMethod(accessors, turn, OutlookDescription, intent);
                }
                else // * Possiamo cominciare un azione di questo scenario, si potranno chiedere ulteriori informazioni all'utente.
                {
                    return ScenarioFactory.FactoryMethod(accessors, turn, Outlook, intent);
                }
            }

            if (isAnAuthIntent(luisServiceResult))
            {
                return ScenarioFactory.FactoryMethod(accessors, turn, Autenticazione, null);
            }

            if (isASupportIntent(luisServiceResult))
            {
                if (intent.EntitiesCollected.Count < intent.EntityLowerBound)
                {
                    return ScenarioFactory.FactoryMethod(accessors, turn, SupportoDescription, intent);
                }
                else
                {
                    return ScenarioFactory.FactoryMethod(accessors, turn, Supporto, intent);
                }
            }

            return ScenarioFactory.FactoryMethod(accessors, turn, Default, intent);
        }

        private static bool isAParkingIntent(LuisServiceResult luisServiceResult)
        {
            var topIntent = luisServiceResult.TopScoringIntent.Item1; // intent
            var score = luisServiceResult.TopScoringIntent.Item2; // score

            return (ParkingIntents.Contains(topIntent) && (score >= 0.75));
        }

        private static bool isAOutlookIntent(LuisServiceResult luisServiceResult)
        {
            var topIntent = luisServiceResult.TopScoringIntent.Item1; // intent
            var score = luisServiceResult.TopScoringIntent.Item2; // score

            return (OutlookIntents.Contains(topIntent) && (score >= 0.75));
        }

        private static bool isAnAuthIntent(LuisServiceResult luisServiceResult)
        {
            var topIntent = luisServiceResult.TopScoringIntent.Item1; // intent
            var score = luisServiceResult.TopScoringIntent.Item2; // score

            return (topIntent.Equals(AutenticazioneIntent) && (score >= 0.75));
        }

        private static bool isASupportIntent(LuisServiceResult luisServiceResult)
        {
            var topIntent = luisServiceResult.TopScoringIntent.Item1; // intent
            var score = luisServiceResult.TopScoringIntent.Item2; // score

            return (topIntent.Equals(RichiestaSupportoIntent) && (score >= 0.75));
        }

    }
}