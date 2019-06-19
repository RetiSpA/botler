using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Botler.Builders;
using Botler.Dialogs.Scenari;
using Botler.Models;
using Botler.Middleware.Services;
using Microsoft.Bot.Builder;
using static Botler.Dialogs.Utility.LuisEntity;
using static Botler.Dialogs.Utility.Scenari;
using static Botler.Dialogs.Utility.LuisIntent;
using Botler.Helpers;
using Newtonsoft.Json;

namespace Botler.Controllers
{
    /// <summary>
    /// This class helps to understand in wich context we are.
    ///
    // 1) Stiamo autenticando l'utente con un magiccode.
    // 2) Trovato uno scenario esecutivo => Lo scenario ha abbastanza informazioni per eseguire le azioni, ma può aver chiesto delle ulteriori informazioni all'utente
    // 3) Trovato uno scenario descrittivo => Forse l'utente vuole inserire nuove informazioni (entità) per compiere l'azione.
    // 4) Nessun contesto trovato, ritorna lo scenario passato dal chiamante.
    /// </summary>

    public static class BotStateContextController
    {

        /// <summary>
        /// Try to find a context in the conversation
        /// </summary>
        /// <param name="scenario">Scenario passed from TurnController </param>
        /// <param name="accessors">MemoryStorage manager class</param>
        /// <param name="turn">Current conversation's turn</param>
        /// <param name="luisServiceResult">LuisResult from this conversation's turn</param>
        /// <returns>A context-based scenario</returns>
        public async static Task<IScenario> CheckBotState(IScenario scenario, BotlerAccessors accessors, ITurnContext turn, LuisServiceResult luisServiceResult)
        {
            var lastBotContext = await FindLastUsefulContextAsync(accessors, turn, scenario);
            IScenario lastUsefulScenario = null;

            if (lastBotContext != null)
            {
                lastUsefulScenario = ScenarioFactory.FactoryMethod(accessors, turn, lastBotContext.scenarioID, lastBotContext.TopIntent);
            }

            var userQuery = turn.Activity.Text;

            // * (4) * // No context found
            if (lastUsefulScenario is null)
            {
                return scenario;
            }

            // * (1) * // Authentication Phase.
            if (AuthenticationHelper.MagicCodeFound(turn.Activity.Text) && lastUsefulScenario.ScenarioID.Equals(Autenticazione))
            {
                return ScenarioFactory.FactoryMethod(accessors, turn, Autenticazione, null);
            }

            // * (2) * // Asking the user for more info.
            if (ExecutionScenarios.Contains(lastUsefulScenario.ScenarioID))
            {
                scenario = CheckLastExecutionScenario(scenario, (ExecutionScenario) lastUsefulScenario, userQuery, lastBotContext.UserQuery,
                    luisServiceResult);
            }

             // * (3) * // Not enough entities to complete de user query.
            if (DescriptionScenarios.Contains(lastUsefulScenario.ScenarioID))
            {
                scenario = CheckLastDescriptionScenario(scenario, (DescriptionScenario) lastUsefulScenario, accessors, turn, luisServiceResult);
            }

            return scenario;
        }

        /// <summary>
        /// Check the last execution scenario found
        /// </summary>
        /// <param name="scenario"></param>
        /// <param name="lastExecutionScenario"></param>
        /// <param name="accessors"></param>
        /// <param name="turn"></param>
        /// <param name="luisServiceResult"></param>
        /// <returns></returns>
        public static IScenario CheckLastExecutionScenario(IScenario scenario, ExecutionScenario lastExecutionScenario, string userQuery, string lastUserQuery, LuisServiceResult luisServiceResult)
        {
            // L'utente ha aggiunto una descrizione per il ticket
            if (lastExecutionScenario.ScenarioID.Equals(Supporto))
            {
                Entity entity = new Entity();
                entity.Type = Descrizione;

                if (luisServiceResult.TopIntent.Name != ConfermaAzioneIntent)
                {
                    entity.Text = userQuery;
                }
                else if (luisServiceResult.TopIntent.Name.Equals(ConfermaAzioneIntent))
                {
                    entity.Text = lastUserQuery;
                }
                scenario.ScenarioIntent.EntitiesCollected.Add(entity);
            }

            if (scenario.ScenarioIntent.EntitiesCollected.Count == 0)
            {
                return scenario;
            }

            // Entità raccolte nell'attuale turno
            foreach (var ent in scenario.ScenarioIntent.EntitiesCollected)
            {
                lastExecutionScenario.ScenarioIntent.EntitiesCollected.Add(ent);
            }
            return lastExecutionScenario;
        }

        /// <summary>
        /// check the last description scenario found
        /// </summary>
        /// <param name="scenario"></param>
        /// <param name="lastDescriptionScenario"></param>
        /// <param name="accessors"></param>
        /// <param name="turn"></param>
        /// <param name="luisServiceResult"></param>
        /// <returns></returns>
        public static IScenario CheckLastDescriptionScenario(IScenario scenario, DescriptionScenario lastDescriptionScenario, BotlerAccessors accessors, ITurnContext turn, LuisServiceResult luisServiceResult)
        {
            if (scenario.ScenarioIntent.EntitiesCollected.Count > 0)
            {

                foreach(var ent in scenario.ScenarioIntent.EntitiesCollected)
                {
                    lastDescriptionScenario.ScenarioIntent.EntitiesCollected.Add(ent);
                }

                if (lastDescriptionScenario.EntityLowerBoundReach())
                {
                    return ChangeScenarioState(accessors, turn, lastDescriptionScenario);
                }

                return lastDescriptionScenario;
            }

            return scenario;
        }

        /// <summary>
        /// Change state, when a Description Scenario reach the Lower Entity Bound, to his Execution Scenario
        /// </summary>
        /// <param name="accessors"></param>
        /// <param name="turn"></param>
        /// <param name="scenarioToChange"></param>
        /// <param name="intent"></param>
        /// <param name="luisServiceResult"></param>
        /// <returns></returns>
        public static IScenario ChangeScenarioState(BotlerAccessors accessors, ITurnContext turn, IScenario lastScenario)
        {
            var scenarioToChange = lastScenario.AssociatedScenario;
            var intent = lastScenario.ScenarioIntent;
            return ScenarioFactory.FactoryMethod(accessors, turn, scenarioToChange, intent);
        }

        /// <summary>
        /// Search for past scenario in this conversation
        /// </summary>
        /// <param name="accessors"></param>
        /// <param name="turn"></param>
        /// <param name="scenario"></param>
        /// <returns></returns>
        // TODO: Pensare anche di trovare scenari inerenti con il corrente intento.
        public static async Task<BotStateContext> FindLastUsefulContextAsync(BotlerAccessors accessors, ITurnContext turn, IScenario scenario)
        {
            string scenarioID = scenario.ScenarioID;
            string convID = turn.Activity.From.Id;
            MongoDBService MongoDB = new MongoDBService();
            IList<BotStateContext> list = await MongoDB.GetAllBotStateByConvIDAsync(convID);

            foreach(var bs in list)
            {
                if ((ExecutionScenarios.Contains(bs.scenarioID) || (DescriptionScenarios.Contains(bs.scenarioID))) && scenario.ScenarioID.Equals(Default))
                {
                    var json = (JsonConvert.SerializeObject(bs));
                    Console.WriteLine("READ STATE -> " + json);
                    return bs;
                }
            }

            return null; // No context found
        }

    }
}