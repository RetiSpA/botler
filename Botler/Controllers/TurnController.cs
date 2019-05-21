using System;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Resources;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Net.Http;
using Microsoft.Bot.Builder.AI.QnA;
using System.Threading.Tasks;
using Botler.Dialogs.Dialoghi;
using Botler.Dialogs.RisorseApi;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Botler.Dialogs.Utility;
using Botler;
using Botler.Dialogs.Scenari;
using Botler.Helper.Commands;
using static Botler.Dialogs.Utility.BotConst;
using static Botler.Dialogs.Utility.ListsResponsesIT;
using static Botler.Dialogs.Utility.Scenari;
using static Botler.Dialogs.Utility.Responses;
using static Botler.Dialogs.Utility.Commands;
using Botler.Model;
using Botler.Services;

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

        public TurnController(BotlerAccessors accessors, BotServices services)
        {
            ILoggerFactory loggerFactory = new LoggerFactory();
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            _services = services ?? throw new ArgumentNullException(nameof(services));
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
            throw new NotImplementedException(nameof(StartEventActivityAsync));
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
                {
                    foreach (var member in currentActivity.MembersAdded)
                    {
                        if (member.Id != currentActivity.Recipient.Id)
                        {
                            ICommand welcomeCommand = CommandFactory.FactoryMethod(currentTurn, _accessors, CommandWelcome);
                            await welcomeCommand.ExecuteCommandAsync();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Manages the message coming from the channel
        /// 1) Get the result from bot's service
        /// 2) Handle a conversation flow interruption
        /// 3) Recognize and execute a command
        /// 4) If any QnA is active get the answer from QnAMaker service
        /// 5) Continue o start a dialog
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task StartMessageActivityAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            // We want to get always a LUIS result first.
            LuisServiceResult luisServiceResult = await CreateLuisServiceResult(cancellationToken);

            var interruptionHandled = await InterruptionRecognizer.InterruptionHandledAsync(luisServiceResult, currentTurn);
            if(interruptionHandled)
            {
                await _accessors.SaveStateAsync(currentTurn);
                return;
            }

            var commandExecuted = await CommandRecognizer.ExecutedCommandFromLuisResultAsync(luisServiceResult, _accessors, currentTurn);
            if(commandExecuted)
            {
                await _accessors.SaveStateAsync(currentTurn);
                return;
            }

            // var qnaActive = await QnAController.CheckQnAIsActive(_accessors, currentTurn);
            // if(qnaActive)
            // {
            // If the user asks a question, if is in our QnA, we send the answer
            if(await QnAController.AnsweredTurnUserQuestionAsync(currentTurn, _accessors, _services))
            {
                await _accessors.SaveStateAsync(currentTurn);
                return;
            }
            //     return;
            // }

            await StartScenarioDialogAsync(luisServiceResult);
        }

        /// <summary>
        /// Create a Data structure that contains the luis result from the turn
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>LuisServiceResult-> [ReccognizeResult->intent,scoring; TopScoringIntent]</returns>
        private async Task<LuisServiceResult> CreateLuisServiceResult(CancellationToken cancellationToken = default(CancellationToken))
        {
            LuisServiceResult luisServiceResult = new LuisServiceResult();

            luisServiceResult.LuisResult = await _services.LuisServices[LuisConfiguration].RecognizeAsync(currentTurn, cancellationToken).ConfigureAwait(false);

            luisServiceResult.TopScoringIntent = luisServiceResult.LuisResult?.GetTopScoringIntent().ToTuple<string,double>();

            return luisServiceResult;
        }

        private async Task StartScenarioDialogAsync(LuisServiceResult luisServiceResult)
        {
            IScenario currentScenario = await ScenarioRecognizer.ExtractCurrentScenarioAsync(luisServiceResult, _accessors, currentTurn);

            await currentScenario.HandleDialogResultStatusAsync(luisServiceResult);

            await _accessors.SaveStateAsync(currentTurn);
        }
    }
}
