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
using Newtonsoft.Json.Linq;
using static Botler.Dialogs.Utility.BotConst;
using static Botler.Dialogs.Utility.ListsResponsesIT;
using static Botler.Dialogs.Utility.Scenari;
using static Botler.Dialogs.Utility.Responses;


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

        private  ScenarioController scenarioController;

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
            scenarioController = new ScenarioController(currentTurn, cancellationToken);

            if (currentActivity.MembersAdded.Any())
                {
                    // Iterate over all new members added to the conversation.
                    foreach (var member in currentActivity.MembersAdded)
                    {
                        if (member.Id != currentActivity.Recipient.Id)
                        {
                             await scenarioController.SendMenuAsync(Welcome);
                        }
                    }
                }
        }

        /// <summary>
        /// Manages the message coming from the channel
        /// 1) Get the result from bot's service
        /// 2) Handle a conversation flow interruption
        /// 3) Continue o start a dialog
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task StartMessageActivityAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            // We want to get always a LUIS result first.
            LuisServiceResult luisServiceResult = await CreateLuisServiceResult(cancellationToken);

            // QnA Service result, in case we find a question.
            QueryResult[] qnaResult = await GetQnAResult();

            if (qnaResult .Length > 0 )
            {
                await  SendQnAAnswerAsync(qnaResult, cancellationToken);
                return;
            }

            scenarioController = new ScenarioController(_accessors, currentTurn, luisServiceResult, cancellationToken: cancellationToken);

            // Check if is in Interrupeted state -> Need to handle interrupts first.
            var isInterrupted =  await scenarioController.CreateResponseForInterruptedStateAsync();

            // Checks if in this turn the user send a command or want to change the scenario, and handle it
            var scenarioChanged = await scenarioController.HandleScenarioChangedAsync();

            if(scenarioChanged)
            {
                return;
            }

            if (isInterrupted != string.Empty) // Send the response based on interruption intent and save the state before the next turn
            {
                await currentTurn.SendActivityAsync(isInterrupted, cancellationToken: cancellationToken);
                await scenarioController.RepromptLastActivityDialogAsync();
                return;
            }
            // Here we may decide to continue with the current scenario(and hence the current dialog) or switch to a different one.
            else await ContinueCurrentDialogAsync(luisServiceResult);
        }

        private async Task SendQnAAnswerAsync(QueryResult[] qnaResult, CancellationToken cancellationToken = default(CancellationToken))
        {
            var answer = qnaResult[0].Answer;
            await currentTurn.SendActivityAsync(answer, cancellationToken: cancellationToken);
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

        /// <summary>
        /// QnA Result
        /// </summary>
        /// <returns></returns>
        private async Task<QueryResult[]> GetQnAResult()
        {
            return await _services.QnAServices[QnAMakerKey].GetAnswersAsync(currentTurn).ConfigureAwait(false);
        }

        /// <summary>
        /// Continue the current dialog or send a none intent response
        /// </summary>
        /// <param name="luisServiceResult"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> ContinueCurrentDialogAsync(LuisServiceResult luisServiceResult)
        {
            // If no one has responded
            if (!currentTurn.Responded)
            {
                var result = await scenarioController.HandleDialogResultStatusAsync();

                if(result is null) // None Intent returned
                {
                    await currentTurn.SendActivityAsync(RandomResponses(NoneResponse));
                }

                return result;
            }
            return null;
        }

    }

    public class LuisServiceResult
    {
        public RecognizerResult LuisResult { get; set; }

        public Tuple<string,double> TopScoringIntent { get; set; }

    }
}
