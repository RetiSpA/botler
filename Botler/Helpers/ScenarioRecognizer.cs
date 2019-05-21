using System;
using System.Threading.Tasks;
using Botler.Dialogs.Scenari;
using Microsoft.Bot.Builder;
using static Botler.Dialogs.Utility.BotConst;
using static Botler.Dialogs.Utility.LuisIntent;
using static Botler.Dialogs.Utility.ListsResponsesIT;
using static Botler.Dialogs.Utility.Responses;
using static Botler.Dialogs.Utility.Scenari;
using Botler.Services;

namespace Botler.Controller
{
    public class ScenarioRecognizer
    {
        public static async Task<IScenario>  ExtractCurrentScenarioAsync(LuisServiceResult luisServiceResult, BotlerAccessors accessors, ITurnContext turn)
        {
            var topIntent = luisServiceResult.TopScoringIntent.Item1; // Intent
            var score = luisServiceResult.TopScoringIntent.Item2; // Score

            IScenario currentScenario = await GetScenarioFromBotStateAsync(accessors, turn);

            if(currentScenario is DefaultScenario)
            {
                IScenario topIntentScenario = GetScenarioFromLuis(luisServiceResult, accessors, turn);

                if(topIntentScenario.NeedAuthentication())
                {
                    var alreadyAuth = await Autenticatore.UserAlreadyAuthAsync(turn, accessors);

                    if(alreadyAuth)
                    {
                        return topIntentScenario;
                    }

                    else
                    {
                        await turn.SendActivityAsync(RandomResponses(AutenticazioneNecessariaResponse));
                        return ScenarioFactory.FactoryMethod(accessors, turn, Autenticazione);
                    }

                }
                return topIntentScenario;
            }

            //return currentScenario.GetType() == topIntentScenario.GetType()  ?  currentScenario : throw new Exception();
            return currentScenario;
        }

        private static  IScenario GetScenarioFromLuis(LuisServiceResult luisServiceResult, BotlerAccessors accessors, ITurnContext turn)
        {
            if(isAParkingIntent(luisServiceResult))
            {
                return ScenarioFactory.FactoryMethod(accessors, turn, Parking);
            }

            return ScenarioFactory.FactoryMethod(accessors, turn, Default);
        }

        private static bool  isAParkingIntent(LuisServiceResult luisServiceResult)
        {
            var topIntent = luisServiceResult.TopScoringIntent.Item1; // intent
            var score = luisServiceResult.TopScoringIntent.Item2; // score

            return (topIntent.Equals(PrenotazioneIntent) ||
                    topIntent.Equals(CancellaPrenotazioneIntent) ||
                    topIntent.Equals(VerificaPrenotazioneIntent) ||
                    topIntent.Equals(TempoRimanentePrenotazioneIntent))
                    && (score >= 0.75);
        }

        private static async Task< IScenario>  GetScenarioFromBotStateAsync(BotlerAccessors accessors, ITurnContext turn)
        {
            string scenarioID = await accessors.ScenarioStateAccessors.GetAsync(turn, () => new string(Default));

            return ScenarioFactory.FactoryMethod(accessors, turn, scenarioID);
        }
    }
}