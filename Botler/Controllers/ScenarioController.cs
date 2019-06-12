using System;
using System.Threading.Tasks;
using Botler.Builders;
using Botler.Dialogs.Scenari;
using Botler.Models;
using Botler.Middleware.Services;
using Microsoft.Bot.Builder;
using static Botler.Dialogs.Utility.BotConst;
using static Botler.Dialogs.Utility.ListsResponsesIT;
using static Botler.Dialogs.Utility.Scenari;
using static Botler.Dialogs.Utility.Responses;
using static Botler.Dialogs.Utility.Commands;
using static Botler.Dialogs.Utility.LuisIntent;

namespace Botler.Controllers
{
    public class ScenarioController
    {
        public static async Task ResponseWithScenarioContextAsync(IScenario scenario, LuisServiceResult luisServiceResult, ITurnContext turn, BotlerAccessors accessors)
        {

        }

         /// <summary>
        /// Viene chiamato quando ci troviamo in uno scenario di descrizione
        /// Di cui abbiamo bisogno di raccogliere entità dall'utente per
        /// completare un azione.
        /// </summary>
        /// <param name="scenario"></param>
        /// <param name="luisServiceResult"></param>
        /// <typeparam name="T"></typeparam>
        // /// <returns></returns>
        // public static async Task EntityCollectedFromScenarioAsync (DescriptionScenario scenario, LuisServiceResult luisServiceResult, ITurnContext turn, BotlerAccessors accessors)
        // {
        //     scenario.SaveEntities(luisServiceResult);

        //     if(scenario.EntityLowerBoundReach(luisServiceResult))
        //     {
        //         Console.WriteLine("Entity bound reach");

        //         // * Ho tutte le entità per rispondere propriamente all'utente * //
        //         // * Si cambia 'stato' si passa allo stato dello scenario Execzxutive * //

        //         ExecutionScenario executiveScenario = ChangeStateToExecutive(turn, accessors, scenario);
        //         await executiveScenario.HandleScenarioStateAsync(turn, accessors, luisServiceResult);
        //         await BotStateBuilder.BuildAndSaveBotStateContextContext(turn, accessors, luisServiceResult, executiveScenario.ScenarioID, executiveScenario.ScenarioIntent);

        //     }
        //     else // * Dovrò pur rispondere in qualche modo * /
        //     {
        //         await BotStateBuilder.BuildAndSaveBotStateContextContext(turn, accessors, luisServiceResult, scenario.ScenarioID, scenario.ScenarioIntent);
        //         // * Rispondi all'utente (e.g) : "Grazie, ma per questa azione ho bisogno di altre informazioni :)"
        //         await turn.SendActivityAsync(scenario.NeedEntityResponse);
        //     }
        // }

        private static ExecutionScenario ChangeStateToExecutive(ITurnContext turn, BotlerAccessors accessors, IScenario scenario)
        {
            ExecutionScenario scenarioExecution = null;
            if (scenario.ScenarioID.Equals(OutlookDescription))
            {
                scenarioExecution = (ExecutionScenario) ScenarioFactory.FactoryMethod(accessors, turn, Outlook, scenario.ScenarioIntent);
            }

            return scenarioExecution;
        }

        public static async Task ExecuteScenarioAction(ExecutionScenario scenario, ITurnContext turn, BotlerAccessors accessors, LuisServiceResult luisServiceResult)
        {
            // * Non è detto che ci sono tutte le entità che l'utente ha inserito, solo nell'ultimo messaggio => Leggere lo stato e trovare scenari con ID della sua controparte  descrittiva * //
            await BotStateBuilder.BuildAndSaveBotStateContextContext(turn, accessors, luisServiceResult, scenario.ScenarioID, scenario.ScenarioIntent);
            await scenario.CreateResponseAsync(luisServiceResult);
        }
        // TODO: Cambiare logica, ridefinire le casistiche.
        public static async Task HandleLowConfidenceScenarioAsyncAsync(ITurnContext turn, BotlerAccessors accessors, IScenario currentScenario, LuisServiceResult luisServiceResult)
        {
            // None intent handler
            var topIntent = luisServiceResult.TopScoringIntent.Item1; // intent
            var score = luisServiceResult.TopScoringIntent.Item2; // score
            // ** Prendiamo l'ultimo stato del bot **/ -> GetLastBotStateByConvID();
            IScenario lastScenario = null;
            var botState = await accessors.GetLastBotStateContextCByConvIDAsync(turn);

            // ** In base allo ScenarioID construiamo l'istanza di scenario ** //
            if (string.IsNullOrEmpty(botState.scenarioID)) Console.WriteLine("null");
            lastScenario = ScenarioFactory.FactoryMethod(accessors, turn, botState.scenarioID, botState.TopIntent);
            // ! Logica da cambiare ! //
            if (lastScenario.ScenarioID != Default)
            {
                await lastScenario.HandleScenarioStateAsync(turn, accessors, luisServiceResult);
            }
            else
            {
                await turn.SendActivityAsync(RandomResponses(NoneResponse));
            }
        }

    }
}