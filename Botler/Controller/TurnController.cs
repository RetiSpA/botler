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
using Newtonsoft.Json.Linq;
using static Botler.Dialogs.Utility.BotConst;
using static Botler.Dialogs.Utility.LuisIntent;
using static Botler.Dialogs.Utility.ListsResponsesIT;
using static Botler.Dialogs.Utility.Responses;
using Botler.Dialogs.Scenari;

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

        private Activity CurrentActivity;

        private ITurnContext CurrentTurn;

        private readonly ScenarioController<IScenario> _scenarioController;

        public TurnController(BotlerAccessors accessors, BotServices services)
        {
            ILoggerFactory loggerFactory = new LoggerFactory();
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _scenarioController = new ScenarioController(_accessors);
        }

        /// <summary>
        ///  Takes the responsability to manage the current turn coming from Botler
        /// </summary>
        /// <param name="turn">Current turn in the channel</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task TurnHandlerAsync(ITurnContext turn, CancellationToken cancellationToken = default(CancellationToken))
        {
            CurrentTurn = turn;
            CurrentActivity = turn.Activity;
            // _scenarioController.CreateDialogContextFromScenarioAsync(turn);

            switch (CurrentActivity.Type)
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Send the welcome card when a member joins the chat
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task StartConversationUpdateActivityAsync(CancellationToken cancellationToken = default(CancellationToken))
        {

            // _scenarioController.CreateWelcomeCard()
            if (CurrentActivity.MembersAdded.Any())
                {
                    // Iterate over all new members added to the conversation.
                    foreach (var member in CurrentActivity.MembersAdded)
                    {
                        // Greet anyone that was not the target (recipient) of this message.
                        // To learn more about Adaptive Cards, see https://aka.ms/msbot-adaptivecards for more details.
                        if (member.Id != CurrentActivity.Recipient.Id)
                        {
                            var welcomeCard = CreateAdaptiveCardAttachment();
                            var response = CreateResponse(CurrentActivity, welcomeCard);
                            await CurrentTurn.SendActivityAsync(response).ConfigureAwait(false);
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

            _scenarioController.InitTurnAccessors(CurrentTurn, luisServiceResult, cancellationToken: cancellationToken);

            // Check if is in Interrupeted state -> Need to handle interrupts first.
            var isInterrupted =  await _scenarioController.CreateResponseForInterruptedStateAsync();

            if (isInterrupted != string.Empty) //Send the response based on interruption intent and save the state before the next turn
            {
                await CurrentTurn.SendActivityAsync(isInterrupted, cancellationToken: cancellationToken);
                await _scenarioController.RepromptLastActivityDialogAsync();
                await SaveState();

                return;
            }
            // Here we may decide to continue with the current scenario(and hence the current dialog) or switch to a different one.
            else await ContinueCurrentDialogAsync(luisServiceResult);

            await SaveState();

        }

        /// <summary>
        /// Create a Data structure that contains the luis result from the turn
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>LuisServiceResult-> [ReccognizeResult->intent,scoring; TopScoringIntent]</returns>
        private async Task<LuisServiceResult> CreateLuisServiceResult(CancellationToken cancellationToken = default(CancellationToken))
        {
            LuisServiceResult luisServiceResult = new LuisServiceResult();

            luisServiceResult.LuisResult =  await _services.LuisServices[LuisConfiguration].RecognizeAsync(CurrentTurn, cancellationToken).ConfigureAwait(false);

            luisServiceResult.TopScoringIntent = luisServiceResult.LuisResult?.GetTopScoringIntent().ToTuple<string,double>();

            return luisServiceResult;

        }
        /// <summary>
        /// QnA Result
        /// </summary>
        /// <returns></returns>
        private async Task<QueryResult[]> GetQnAResult()
        {
            return await  _services.QnAServices[QnAMakerKey].GetAnswersAsync(CurrentTurn).ConfigureAwait(false);
        }

        /// <summary>
        /// Save the conversation and user states
        /// </summary>
        /// <returns></returns>
        private async Task SaveState()
        {
            await _accessors.SaveConvStateAsync(CurrentTurn);
            await _accessors.SaveUserStateAsyn(CurrentTurn);
        }

        /// <summary>
        /// Continue the current dialog or send a none intent response
        /// </summary>
        /// <param name="luisServiceResult"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> ContinueCurrentDialogAsync(LuisServiceResult luisServiceResult)
        {
            // If no one has responded
            if (!CurrentTurn.Responded)
            {
                var result =  await _scenarioController.HandleDialogResultStatusAsync();

                if(result is null) // None Intent returned
                {
                    await CurrentTurn.SendActivityAsync(RandomResponses(NoneResponse));
                }

                return result;
            }
            return null;
        }

        private Activity CreateResponse(Activity activity, Attachment attachment)
        {
            var response = activity.CreateReply();
            response.Attachments = new List<Attachment>() { attachment };
            return response;
        }

        private Attachment CreateAdaptiveCardAttachment()
        {
            var adaptiveCard = File.ReadAllText(@".\Dialogs\Welcome\Resources\welcomeCard.json");
            return new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCard),
            };
        }
    }

    public class LuisServiceResult
    {
        public RecognizerResult LuisResult { get; set; }

        public Tuple<string,double> TopScoringIntent { get; set; }

    }




}
