// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Botler.Dialogs.Dialoghi;
using Botler.Dialogs.RisorseApi;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Botler
{
    /// <summary>
    /// Main entry point and orchestration for bot.
    /// </summary>
    public class BasicBot : IBot
    {
        // Supported LUIS Intents
        public const string PresentazioneIntent = "Presentazione";
        public const string PrenotazioneIntent = "Prenotazione";
        public const string CancellaPrenotazioneIntent = "Cancellazione";
        public const string TempoRimanentePrenotazioneIntent = "TempoRimanentePrenotazione";

        public const string InformazioniIntent = "Informazioni";
        public const string PossibilitàIntent = "Possibilità";
        public const string NoneIntent = "None";

        /// <summary>
        /// Key in the bot config (.bot file) for the LUIS instance.
        /// In the .bot file, multiple instances of LUIS can be configured.
        /// </summary>
        public static readonly string LuisConfiguration = "basic-bot-LUIS";

        private readonly IStatePropertyAccessor<PresentazioneModel> _presentazioneStateAccessor;
        private readonly IStatePropertyAccessor<PrenotazioneModel> _prenotazioneStateAccessor;
        private readonly IStatePropertyAccessor<PrenotazioneModel> _cancellaPrenotazioneStateAccessor;
        private readonly IStatePropertyAccessor<PrenotazioneModel> _visualizzaTempoStateAccessor;

        private readonly IStatePropertyAccessor<DialogState> _dialogStateAccessor;
        private readonly UserState _userState;
        private readonly ConversationState _conversationState;
        private readonly BotServices _services;

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicBot"/> class.
        /// </summary>
        /// <param name="botServices">Bot services.</param>
        /// <param name="accessors">Bot State Accessors.</param>
        public BasicBot(BotServices services, UserState userState, ConversationState conversationState, ILoggerFactory loggerFactory)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _userState = userState ?? throw new ArgumentNullException(nameof(userState));
            _conversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));

            _dialogStateAccessor = _conversationState.CreateProperty<DialogState>(nameof(DialogState));
            _presentazioneStateAccessor = _userState.CreateProperty<PresentazioneModel>(nameof(PresentazioneModel));
            _prenotazioneStateAccessor = _userState.CreateProperty<PrenotazioneModel>(nameof(PrenotazioneModel));
            _cancellaPrenotazioneStateAccessor = _userState.CreateProperty<PrenotazioneModel>(nameof(PrenotazioneModel));
            _visualizzaTempoStateAccessor = _userState.CreateProperty<PrenotazioneModel>(nameof(PrenotazioneModel));

            // Verify LUIS configuration.
            if (!_services.LuisServices.ContainsKey(LuisConfiguration))
            {
                throw new InvalidOperationException($"The bot configuration does not contain a service type of `luis` with the id `{LuisConfiguration}`.");
            }

            Dialogs = new DialogSet(_dialogStateAccessor);
            Dialogs.Add(new Presentazione(_presentazioneStateAccessor, loggerFactory));
            Dialogs.Add(new Prenotazione(_prenotazioneStateAccessor, loggerFactory));
            Dialogs.Add(new CancellaPrenotazione(_cancellaPrenotazioneStateAccessor, loggerFactory));
            Dialogs.Add(new VisualizzazioneTempo(_visualizzaTempoStateAccessor, loggerFactory));
        }

        private DialogSet Dialogs { get; set; }

        /// <summary>
        /// Run every turn of the conversation. Handles orchestration of messages.
        /// </summary>
        /// <param name="turnContext">Bot Turn Context.</param>
        /// <param name="cancellationToken">Task CancellationToken.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var activity = turnContext.Activity;

            // Create a dialog context
            var dc = await Dialogs.CreateContextAsync(turnContext);

            if (activity.Type == ActivityTypes.Message)
            {
                // Perform a call to LUIS to retrieve results for the current activity message.
                var luisResults = await _services.LuisServices[LuisConfiguration].RecognizeAsync(dc.Context, cancellationToken).ConfigureAwait(false);

                // If any entities were updated, treat as interruption.
                // For example, "no my name is tony" will manifest as an update of the name to be "tony".
                var topScoringIntent = luisResults?.GetTopScoringIntent();

                var topIntent = topScoringIntent.Value.intent;

                // update greeting state with any entities captured
                await UpdatePresentazioneState(luisResults, dc.Context);

                // Handle conversation interrupts first.
                var interrupted = await IsTurnInterruptedAsync(dc, topIntent);
                if (interrupted)
                {
                    // Bypass the dialog.
                    // Save state before the next turn.
                    await _conversationState.SaveChangesAsync(turnContext);
                    await _userState.SaveChangesAsync(turnContext);
                    return;
                }

                // Continue the current dialog
                var dialogResult = await dc.ContinueDialogAsync();

                // if no one has responded,
                if (!dc.Context.Responded)
                {
                    // examine results from active dialog
                    switch (dialogResult.Status)
                    {
                        case DialogTurnStatus.Empty:
                            switch (topIntent)
                            {
                                case PresentazioneIntent:
                                    await dc.BeginDialogAsync(nameof(Presentazione));
                                    break;

                                case PrenotazioneIntent:
                                    await dc.BeginDialogAsync(nameof(Prenotazione));
                                    break;

                                case CancellaPrenotazioneIntent:
                                    await dc.BeginDialogAsync(nameof(CancellaPrenotazione));
                                    break;

                                case TempoRimanentePrenotazioneIntent:
                                    await dc.BeginDialogAsync(nameof(VisualizzazioneTempo));
                                    break;

                                case NoneIntent:
                                default:
                                    // Help or no intent identified, either way, let's provide some help.
                                    // to the user
                                    await dc.Context.SendActivityAsync("Non capisco ciò che mi stai dicendo, mi spiace.");
                                    break;
                            }

                            break;

                        case DialogTurnStatus.Waiting:
                            // The active dialog is waiting for a response from the user, so do nothing.
                            break;

                        case DialogTurnStatus.Complete:
                            await dc.EndDialogAsync();
                            break;

                        default:
                            await dc.CancelAllDialogsAsync();
                            break;
                    }
                }
            }
            else if (activity.Type == ActivityTypes.ConversationUpdate)
            {
                if (activity.MembersAdded.Any())
                {
                    // Iterate over all new members added to the conversation.
                    foreach (var member in activity.MembersAdded)
                    {
                        // Greet anyone that was not the target (recipient) of this message.
                        // To learn more about Adaptive Cards, see https://aka.ms/msbot-adaptivecards for more details.
                        if (member.Id != activity.Recipient.Id)
                        {
                            var welcomeCard = CreateAdaptiveCardAttachment();
                            var response = CreateResponse(activity, welcomeCard);
                            await dc.Context.SendActivityAsync(response).ConfigureAwait(false);
                        }
                    }
                }
            }

            await _conversationState.SaveChangesAsync(turnContext);
            await _userState.SaveChangesAsync(turnContext);
        }

        // Determine if an interruption has occured before we dispatch to any active dialog.
        private async Task<bool> IsTurnInterruptedAsync(DialogContext dc, string topIntent)
        {
            // See if there are any conversation interrupts we need to handle.

            if (topIntent.Equals(InformazioniIntent))
            {
                await dc.Context.SendActivityAsync("Sono Bot Reti! Sono qui per aiutarti a prenotare un parcheggio");
                if (dc.ActiveDialog != null)
                {
                    await dc.RepromptDialogAsync();
                }

                return true;        // Handled the interrupt.
            }

            if (topIntent.Equals(PossibilitàIntent))
            {
                await dc.Context.SendActivityAsync("Sono stato progettato per gestire le seguenti mansioni:\n-\tPrenotazione di un parcheggio\n-\tVisualizzazione di una prenotazione\n-\tTempo rimanente relativo alla prenotazione\n-\tCancellazione di una prenotazione attiva");

                if (dc.ActiveDialog != null)
                {
                    await dc.RepromptDialogAsync();
                }

                return true;        // Handled the interrupt.
            }

            return false;           // Did not handle the interrupt.
        }

        // Create an attachment message response.
        private Activity CreateResponse(Activity activity, Attachment attachment)
        {
            var response = activity.CreateReply();
            response.Attachments = new List<Attachment>() { attachment };
            return response;
        }

        // Load attachment from file.
        private Attachment CreateAdaptiveCardAttachment()
        {
            var adaptiveCard = File.ReadAllText(@".\Dialogs\Welcome\Resources\welcomeCard.json");
            return new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCard),
            };
        }

        /// <summary>
        /// Helper function to update presentazione state with entities returned by LUIS.
        /// </summary>
        /// <param name="luisResult">LUIS recognizer <see cref="RecognizerResult"/>.</param>
        /// <param name="turnContext">A <see cref="ITurnContext"/> containing all the data needed
        /// for processing this conversation turn.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private async Task UpdatePresentazioneState(RecognizerResult luisResult, ITurnContext turnContext)
        {
            if (luisResult.Entities != null && luisResult.Entities.HasValues)
            {
                // Get latest GreetingState
                var presentazioneState = await _presentazioneStateAccessor.GetAsync(turnContext, () => new PresentazioneModel());
                var entities = luisResult.Entities;

                // Supported LUIS Entities
                string[] userNameEntities = { "nome", "nome_paternAny" };

                // Update any entities
                // Note: Consider a confirm dialog, instead of just updating.
                foreach (var name in userNameEntities)
                {
                    // Check if we found valid slot values in entities returned from LUIS.
                    if (entities[name] != null)
                    {
                        // Capitalize and set new user name.
                        var newName = (string)entities[name][0];
                        presentazioneState.Name = char.ToUpper(newName[0]) + newName.Substring(1);
                        break;
                    }
                }

                // Set the new values into state.
                await _presentazioneStateAccessor.SetAsync(turnContext, presentazioneState);
            }
        }
    }
}
