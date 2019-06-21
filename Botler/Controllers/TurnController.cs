using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Botler.Dialogs.Scenari;
using Botler.Commands;
using Botler.Models;
using Botler.Builders;
using Botler.Helpers;
using Botler.Middleware.Services;
using static Botler.Dialogs.Utility.Commands;
using Botler.Controllers;

/// <summary>
/// This class takes the responsability
/// to handle the current turnContext in
/// Botler, a TurnContext contains all the information
/// to manage an Acitivity (Message, Event, ConversationUpdate, Invoke)
/// when necessary.
/// </summary>

namespace Botler.Controller
{
    public class TurnController
    {
        private readonly BotlerAccessors _accessors;

        private readonly BotServices _services;

        private Activity currentActivity;

        private ITurnContext currentTurn;

        private BotStateContext currentBotState;

        private MongoDBService mongoDB;

        public TurnController(BotlerAccessors accessors, BotServices services, MongoDBService mongoDBService)
        {
            ILoggerFactory loggerFactory = new LoggerFactory();
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            _services = services ?? throw new ArgumentNullException(nameof(services));

            currentBotState = new BotStateContext();

            mongoDB = mongoDBService;
        }

        /// <summary>
        ///  Takes the responsability to manage the current turn coming from Botler
        /// </summary>
        /// <param name="turn">Current turn in the channel</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task TurnHandlerAsync(ITurnContext turn, CancellationToken cancellationToken = default(CancellationToken))
        {
            currentTurn = turn;
            currentActivity = turn.Activity;

            switch (currentActivity.Type)
            {
                case ActivityTypes.Message:
                    await StartMessageActivityAsync(cancellationToken: cancellationToken);
                    break;
                case ActivityTypes.ConversationUpdate:
                    await StartConversationUpdateActivityAsync(cancellationToken: cancellationToken);
                    break;
                case ActivityTypes.Event:
                    await StartEventActivityAsync(cancellationToken: cancellationToken);
                    break;
            }
        }

        /// <summary>
        /// Not Implemented yet
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task StartEventActivityAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            // LuisServiceResult luisServiceResult = await CreateLuisServiceResult(cancellationToken);
            // currentBotState = await _accessors.GetLastBotStateContextCByConvIDAsync(currentTurn);
            // IScenario scenarioAuth = ScenarioFactory.FactoryMethod(_accessors, currentTurn, currentBotState.scenarioID, null);
            // await scenarioAuth.HandleScenarioStateAsync(currentTurn, _accessors, luisServiceResult);
            await AuthenticationHelper.SecondPhaseAuthAsync(currentTurn, _accessors);
        }

        /// <summary>
        /// Send the welcome card when a member joins the chat
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task StartConversationUpdateActivityAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (currentActivity.MembersAdded.Any())
            {
                foreach (var member in currentActivity.MembersAdded)
                {
                    if (member.Id != currentActivity.Recipient.Id)
                    {
                        // Create the first BostStateContext inhert to the first turn (i = 0)
                        currentBotState = await BotStateBuilder.BuildFirstTurnBotStateContext(_accessors, currentTurn);
                        // And then insert the JSON Document into the Azure MongoDB
                        await mongoDB.InsertJSONContextDocAsync(currentBotState);

                        ICommand welcomeCommand = CommandFactory.FactoryMethod(currentTurn, _accessors, CommandWelcome);
                        await welcomeCommand.ExecuteCommandAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Manages the message coming from the channel
        /// * 1) Get the result from bot's service
        /// * 2) Handle a conversation flow interruption
        /// * 3) Recognize and execute a command
        /// * 4) If any QnA is active get the answer from QnAMaker service
        /// * 5) Continue o start a dialog
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task StartMessageActivityAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            // We want to get always a LUIS result first.
            LuisServiceResult luisServiceResult = await LuisServiceResultBuilder.CreateLuisServiceResult(currentTurn, _services, cancellationToken);
            // Update the turnCouter of this conversation
            await _accessors.UpdateTurnCounterAsync(currentTurn);

            // Analyze LuisServiceResult to create a BotStateContext if needed, based on intents(and top intent)

            if (await InterruptionRecognizer.InterruptionHandledAsync(luisServiceResult, currentTurn))
            {
                await _accessors.SaveStateAsync(currentTurn);
                // ? Reply with a response based on past states/context ?
                return;
            }

            if (await CommandRecognizer.ExecutedCommandFromLuisResultAsync(luisServiceResult, _accessors, currentTurn))
            {
                await _accessors.SaveStateAsync(currentTurn);
                return;
            }

            if (await QnAController.AnsweredTurnUserQuestionAsync(currentTurn, _accessors, _services))
            {
                await _accessors.SaveStateAsync(currentTurn);
                return;
            }

             // Continue or start a new dialog of a scenario or a context based dialog
            await CreateScenarioResponseWithLuisAsync(luisServiceResult);
        }

        /// <summary>
        /// Create a response for the current turn  checking the LuisResult and bot context
        /// </summary>
        /// <param name="luisServiceResult"> LuisResult and all the entities found </param>
        /// <returns></returns>
        private async Task CreateScenarioResponseWithLuisAsync(LuisServiceResult luisServiceResult)
        {
            IScenario currentScenario = await ScenarioRecognizer.ExtractCurrentScenarioAsync(luisServiceResult, _accessors, currentTurn);

            // * Gestisce lo scenario in base al suo contesto  * //
            await currentScenario.HandleScenarioStateAsync(currentTurn, _accessors, luisServiceResult);

            // * Salva lo stato di questo turno nel cosmbosDB * //
            await BotStateBuilder.BuildAndSaveBotStateContextContext(currentTurn, _accessors, luisServiceResult, currentScenario);

            // * Salva lo stato del MemoryStorage * //
            await _accessors.SaveStateAsync(currentTurn);
        }

    }
}
