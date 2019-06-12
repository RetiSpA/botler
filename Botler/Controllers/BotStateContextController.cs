using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Botler.Builders;
using Botler.Dialogs.Scenari;
using Botler.Models;
using Botler.Middleware.Services;
using Microsoft.Bot.Builder;
using static Botler.Dialogs.Utility.LuisIntent;
using static Botler.Dialogs.Utility.Scenari;
using Botler.Helpers;

namespace Botler.Controllers
{
    /// <summary>
    /// This class helps to understand in wich context we are ( LEGGE IL PASSATO DELLA CONVERSAZIONE)
    /// </summary>
    // * Casistica Context * //
    // * 1) Nessun contesto precedente diverso dallo scenario default trovato o associabile a questo -> primo scenario utile per iniziare un contesto
    // * 2) Trovato uno scenario esecutivo => Lo scenario ha abbastanza informazioni per eseguire le azioni,ma  ha chiesto delle ulteriori informazioni all'utente
    // * 3) Trovato uno scenario descrittivo => Forse l'utente vuole inserire nuove informazioni (entità) per compiere l'azione.
  
    //  ! Uno scenario descrittivo non arriverà mai da luis, ma andrà creato in base ad uno scenario default o esecutivo
    // ! Di qui passerà sempre e solo uno scenario di tipo DEFAULT, perchè gli altri verranno riconosciuto prima. Ma si possono avere intent con basso score.

    public static class BotStateContextController
    {

        // Viene chiamato da ScenarioRecognizer in caso venga riconosciuto uno scenario default
        public async static Task<IScenario> CheckBotState(IScenario scenario, BotlerAccessors accessors, ITurnContext turn, LuisServiceResult luisServiceResult)
        {
            var lastUsefulScenario = await FindLastUsefulContextAsync(accessors, turn, scenario);

            // * (1) * //
            if (lastUsefulScenario is null)
            {
                return scenario;
            }
            // * (2) * //
            // * Last Scenario is Execution Type -> Vediamo se ci sono altre entità, ed eseguiamo le azioni con quello che abbiamo.s
            if (ExecutionScenarios.Contains(lastUsefulScenario.ScenarioID))
            {
                scenario = CheckLastExecutionScenario(scenario, (ExecutionScenario) lastUsefulScenario, accessors, turn, luisServiceResult);
            }
            // * (3) * //
            // * L'ultimo scenario è di tipo descrittivo, quindi non è stato effettuato il cambio di scenario e servono altre informazioni
            if (DescriptionScenarios.Contains(lastUsefulScenario.ScenarioID))
            {
                scenario = CheckLastDescriptionScenario(scenario, (DescriptionScenario) lastUsefulScenario, accessors, turn, luisServiceResult);
            }

            return scenario;
        }

        public static IScenario CheckLastExecutionScenario(IScenario scenario, ExecutionScenario lastExecutionScenario, BotlerAccessors accessors, ITurnContext turn, LuisServiceResult luisServiceResult)
        {
            if (scenario.ScenarioIntent.EntitiesCollected.Count == 0)
            {
                return scenario;
            }
            // Entità raccolte nell'attuale turno
            foreach(var ent in scenario.ScenarioIntent.EntitiesCollected)
            {
                lastExecutionScenario.ScenarioIntent.EntitiesCollected.Add(ent);
            }
            return lastExecutionScenario;
        }
        public static IScenario CheckLastDescriptionScenario(IScenario scenario, DescriptionScenario lastDescriptionScenario, BotlerAccessors accessors, ITurnContext turn, LuisServiceResult luisServiceResult)
        {
            if (scenario.ScenarioIntent.EntitiesCollected.Count > 0)
            {
                scenario.ScenarioIntent.EntitiesCollected = // Check if the entities collected from this turn are useful for the context
                    EntityFormatHelper.FiltrEntityByIntent(lastDescriptionScenario.ScenarioIntent.Name, scenario.ScenarioIntent.EntitiesCollected);

                foreach(var ent in scenario.ScenarioIntent.EntitiesCollected)
                {
                    lastDescriptionScenario.ScenarioIntent.EntitiesCollected.Add(ent);
                }

                if (lastDescriptionScenario.EntityLowerBoundReach())
                {
                    return BotStateContextController.ChangeScenarioState(accessors, turn, lastDescriptionScenario.AssociatedScenario, lastDescriptionScenario.ScenarioIntent, luisServiceResult);
                }

                return lastDescriptionScenario;
            }
            return scenario;
        }

        public static IScenario ChangeScenarioState(BotlerAccessors accessors, ITurnContext turn, string scenarioToChange, Intent intent, LuisServiceResult luisServiceResult)
        {
            return ScenarioFactory.FactoryMethod(accessors, turn, scenarioToChange, intent);
        }

        public static async Task<IScenario> FindLastUsefulContextAsync(BotlerAccessors accessors, ITurnContext turn, IScenario scenario)
        {
            string scenarioID = scenario.ScenarioID;
            string convID = turn.Activity.From.Id;
            MongoDBService MongoDB = new MongoDBService();
            IList<BotStateContext> list = await MongoDB.GetAllBotStateByConvIDAsync(convID);

            foreach(var bs in list)
            {
                if ((ExecutionScenarios.Contains(bs.scenarioID) || (DescriptionScenarios.Contains(bs.scenarioID))) && scenario.ScenarioID.Equals(Default))
                {
                    return ScenarioFactory.FactoryMethod(accessors, turn, bs.scenarioID, bs.TopIntent);
                }
            }
            return null; // No context found
        }

    }
}
