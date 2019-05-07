

using System;
using System.Threading.Tasks;
using Botler.Dialogs.Scenari;
using Microsoft.Bot.Builder;
using static Botler.Dialogs.Utility.BotConst;
using static Botler.Dialogs.Utility.LuisIntent;
using static Botler.Dialogs.Utility.ListsResponsesIT;
using static Botler.Dialogs.Utility.Responses;
using static Botler.Dialogs.Utility.Scenari;

namespace Botler.Controller
{
    public class ScenarioRecognizer
    {
        public static  async Task<IScenario>  ExtractCurrentScenarioAsync(LuisServiceResult luisServiceResult, BotlerAccessors accessors, ITurnContext turn)
        {
            var topIntent = luisServiceResult.TopScoringIntent.Item1; // Intent
            var score = luisServiceResult.TopScoringIntent.Item2; // Score

            IScenario currentScenario = await  ScenarioFactoryFromBotStateAsync(accessors, turn);

            IScenario topIntentScenario =  ScenarioFactoryFromLuisIntent(luisServiceResult, accessors);

            return currentScenario.GetType() == topIntentScenario.GetType()  ?  currentScenario : throw new Exception();
        }

        private static  IScenario ScenarioFactoryFromLuisIntent(LuisServiceResult luisServiceResult, BotlerAccessors accessors)
        {
            bool parkingScenario = isAParkingIntent(luisServiceResult);

            //bool  autenticazioneScenario = isAnAuthIntent(luisServiceResult);

            if(parkingScenario)
            {
                return new ParkingScenario(accessors);
            }

            return new DefaultScenario(accessors);

        }

        private static bool  isAParkingIntent(LuisServiceResult luisServiceResult)
        {
            var topIntent = luisServiceResult.TopScoringIntent.Item1; // intent
            var score = luisServiceResult.TopScoringIntent.Item2; // score

            return ( topIntent.Equals(PrenotazioneIntent) ||
                           topIntent.Equals(CancellaPrenotazioneIntent) ||
                           topIntent.Equals(VerificaPrenotazioneIntent) ||
                           topIntent.Equals(TempoRimanentePrenotazioneIntent))
                           && (score > 0.75);
        }

        private static async Task< IScenario>  ScenarioFactoryFromBotStateAsync(BotlerAccessors accessors, ITurnContext turn)
        {
            string scenarioID = await  accessors.ScenarioStateAccessors.GetAsync(turn, () => new string(Default));

           if (scenarioID.Equals(Default))
            {
                return new DefaultScenario(accessors);
            }

            if (scenarioID.Equals(Parking))
            {
                return new ParkingScenario(accessors);
            }

            if(scenarioID.Equals(Autenticazione))
            {
                return new AutenticazioneScenario(accessors);
            }

            return new DefaultScenario(accessors);

        }
    }
}