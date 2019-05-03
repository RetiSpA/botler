using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Botler.Dialogs.Scenari;
using Botler.Dialogs.Dialoghi;
using Botler.Dialogs.RisorseApi;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Botler.Controller;
using Botler.Dialogs.Utility;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Attachment = Microsoft.Bot.Schema.Attachment;
using static Botler.Dialogs.Utility.BotConst;
using static Botler.Dialogs.Utility.LuisIntent;
using static Botler.Dialogs.Utility.ListsResponsesIT;
using static Botler.Dialogs.Utility.Responses;

using static Botler.Dialogs.Utility.Scenari;

namespace Botler.Dialogs.Scenari
{
    public class ScenarioController
    {
        private readonly BotlerAccessors _accessors;

        private readonly Autenticatore _autenticatore;

        private DialogContext currentDialogContext;

        private IScenario currentScenario;

        private ITurnContext currentTurn;

        private LuisServiceResult luisResult;

        private CancellationToken cancellationToken;

        public ScenarioController(BotlerAccessors accessors)
        {
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));

            currentScenario = new DefaultScenario(_accessors);

            _autenticatore = new Autenticatore();
        }

        /// <summary>
        /// This method will initialize all the property of the current ITurnContext from TurnController class
        /// </summary>
        /// <param name="currentTurn"></param>
        /// <param name="luisServiceResult">Luis RecognizeResult and TopIntent and TopScoring</param>
        /// <param name="cancellationToken"></param>
        public void InitTurnAccessors(ITurnContext currentTurn, LuisServiceResult luisServiceResult, CancellationToken cancellationToken)
        {
            this.currentTurn = currentTurn ?? throw new ArgumentNullException(nameof(currentTurn));
            this.luisResult = luisServiceResult ?? throw new ArgumentNullException(nameof(luisServiceResult));
            this.cancellationToken = cancellationToken;

        }

        /// <summary>
        /// Create a response for all kind of Interrupeted intent, and recognize the 'Autenticazione' command
        /// </summary>
        /// <returns>A string that will be send to user</returns>
        public async Task<string> CreateResponseForInterruptedStateAsync()
        {
            var topIntent = luisResult.TopScoringIntent.Item1; // intent
            var score = luisResult.TopScoringIntent.Item2; // score
            await CreateDialogContextFromScenarioAsync();

            if (topIntent.Equals(PresentazioneIntent) && (score > 0.75))
            {
                string response = RandomResponses(PresentazioneResponse);

                return response;        // Handled the interrupt.
            }

            if (topIntent.Equals(GoodbyeIntent) && (score > 0.75))
            {
                string response = RandomResponses(SalutoResponse);

                return response;        // Handled the interrupt.
            }

            if (topIntent.Equals(InformazioniIntent) && (score > 0.75))
            {
                string response = RandomResponses(InformazioneResponse);

                return response;        // Handled the interrupt.
            }

            if (topIntent.Equals(RingraziamentiIntent) && (score > 0.75))
            {
                string response = RandomResponses(RingraziamentoResponse);

                return response;        // Handled the interrupt.
            }

            if (topIntent.Equals(SalutePositivoIntent) && (score > 0.75))
            {
                string response = RandomResponses(SalutoPositivoResponse);

                return response;        // Handled the interrupt.
            }

            if (topIntent.Equals(SaluteNegativoIntent) && (score > 0.75))
            {
                string response = RandomResponses(SalutoNegativoResponse);

                return response;        // Handled the interrupt.
            }

            if (topIntent.Equals(AnomaliaIntent) && (score > 0.75))
            {
                string response = RandomResponses(AnomaliaResponse);

                return response;        // Handled the interrupt.
            }

            if (topIntent.Equals(PossibilitàIntent) && (score > 0.75))
            {
                string response = RandomResponses(PossibilitaParcheggioResponse);

                return response;        // Handled the interrupt.
            }

            return string.Empty;           // Did not handle
        }

        public async Task<bool> HandleCommandAsync()
        {
            var topIntent = luisResult.TopScoringIntent.Item1; // intent
            var score = luisResult.TopScoringIntent.Item2; // score
            var message = currentTurn.Activity.Text;

            // The user want to SignIn
            if ((topIntent.Equals(Autenticazione) && (score > 0.75)) || _autenticatore.MagicCodeFound(currentTurn.Activity.Text))
            {
                await _accessors.ScenarioStateAccessors.SetAsync(currentTurn, Autenticazione);
                var alreadyAuth = await _accessors.AutenticazioneDipedenteAccessors.GetAsync(currentTurn, () => false, cancellationToken);
                await SaveState();

                if(alreadyAuth)
                {
                    return false;
                }

                else
                {
                   await HandleDialogResultStatusAsync();
                   return true;
                }
            }
            // The user want to park a car
            if ((topIntent.Equals(Parking) && (score > 0.75)) ||  message.Equals(Parking))
            {
                await _accessors.ScenarioStateAccessors.SetAsync(currentTurn, Parking);
                await SaveState();
                await SendMenuParkingHeroCard();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Manages the current state of the DialogStack
        /// </summary>
        /// <returns></returns>
        public async Task<DialogTurnResult> HandleDialogResultStatusAsync()
        {
            await RetrieveScenarioStateAsync(); // Read the BotlerAccessors
            await CreateDialogContextFromScenarioAsync();
            var dialogResult = await currentDialogContext.ContinueDialogAsync();

            switch (dialogResult.Status)
                {
                    case DialogTurnStatus.Empty:
                        return await StartDialog();

                    case DialogTurnStatus.Waiting:
                        // Do Nothing
                        break;
                    case DialogTurnStatus.Complete:
                        return await currentDialogContext.EndDialogAsync();

                    default:
                        return await currentDialogContext.CancelAllDialogsAsync();

                }
            return null;
        }

        /// <summary>
        /// After and interruption, this will send the last Dialog Prompt, if there is any
        /// </summary>
        /// <returns></returns>
        public async Task RepromptLastActivityDialogAsync()
        {
            if (currentDialogContext.ActiveDialog != null)
            {
                await currentDialogContext.RepromptDialogAsync();
            }
        }

        /// <summary>
        /// Starts CurrentScenario's Dialog
        /// </summary>
        /// <returns></returns>
        private async Task<DialogTurnResult> StartDialog()
        {
            await RetrieveScenarioStateAsync();
            await CreateDialogContextFromScenarioAsync();

            if(currentScenario.GetType() == typeof(ParkingScenario))
            {
                return await StartParkingDialog();
            }

            if(currentScenario.GetType() == typeof(AutenticazioneScenario))
            {
                await AuthPhasesAsync();
            }

            return null;
        }

        private async Task<DialogTurnResult> StartParkingDialog()
        {
            var topIntent = luisResult.TopScoringIntent.Item1;
            Console.WriteLine(topIntent);
            switch (topIntent)
            {
                case PrenotazioneIntent:
                    return await currentDialogContext.BeginDialogAsync(nameof(Prenotazione));

                case CancellaPrenotazioneIntent:
                    return await currentDialogContext.BeginDialogAsync(nameof(CancellaPrenotazione));

                case TempoRimanentePrenotazioneIntent:
                    return await currentDialogContext.BeginDialogAsync(nameof(VisualizzaTempo));

                case VerificaPrenotazioneIntent:
                    return await currentDialogContext.BeginDialogAsync(nameof(VisualizzaPrenotazione));

                case NoneIntent:
                    default:
                    return null;
            }
        }

        private async Task CreateDialogContextFromScenarioAsync()
        {
            DialogSet dialogSet = currentScenario.GetDialogSet();
            currentDialogContext = await dialogSet.CreateContextAsync(currentTurn);
        }

        /// <summary>
        /// Read the BotlerAccessors to read the actual Scenario, if is null, it will create the dummy Scenario:
        /// DefaultScenario
        /// </summary>
        /// <returns></returns>
        private async Task RetrieveScenarioStateAsync()
        {
            var scenarioID = await _accessors.ScenarioStateAccessors.GetAsync(currentTurn, () => new string(Default), cancellationToken);
            currentScenario = await ScenarioFactory(scenarioID);
        }

        private async Task SaveState()
        {
            await _accessors.SaveConvStateAsync(currentTurn);
            await _accessors.SaveUserStateAsyn(currentTurn);
        }

        private async Task<IScenario> ScenarioFactory(string scenarioID)
        {
            if (scenarioID.Equals(Default))
            {
                return new DefaultScenario(_accessors);
            }

            if (scenarioID.Equals(Parking))
            {
                return new ParkingScenario(_accessors);
            }

            if(scenarioID.Equals(Autenticazione))
            {
                return new AutenticazioneScenario(_accessors);
            }

            return new DefaultScenario(_accessors);
        }

        /// <summary>
        /// Manage the authentication.
        /// 1) Sends the OAuthCard
        /// 2) Verify MagicCode
        /// </summary>
        /// <returns></returns>
        private async Task<bool> AuthPhasesAsync()
        {
            var messageText = currentTurn.Activity.Text;
            var magicCodeReceived = _autenticatore.MagicCodeFound(messageText);

            // Second part of Authentication (MagicCode or Token validation)
            if (magicCodeReceived)
            {
                return await SecondPhaseAuthAsync();
            }

            // First part of Authentication (Sends OAuthCard)
            else
            {
                return await FirstPhaseAuthAsync();
            }
        }

        private async Task<bool> FirstPhaseAuthAsync()
        {
            await _accessors.ScenarioStateAccessors.SetAsync(currentTurn, Autenticazione);
            Activity card = _autenticatore.CreateOAuthCard(currentTurn);
            await currentTurn.SendActivityAsync(card, cancellationToken).ConfigureAwait(false);
            return false;
        }

        private async Task<bool> SecondPhaseAuthAsync()
        {
            var adapter = (BotFrameworkAdapter) currentTurn.Adapter;
            var message = currentTurn.Activity.AsMessageActivity();
            var response = string.Empty;

            var tokenResponse = _autenticatore.RecognizeTokenAsync(currentTurn, adapter, cancellationToken);

                if (tokenResponse != null) // Autenticazione Succeded
                {
                    // Changes the CurrentDialog
                    await _accessors.AutenticazioneDipedenteAccessors.SetAsync(currentTurn, true);
                    await _accessors.ScenarioStateAccessors.SetAsync(currentTurn, Default);
                    await RetrieveScenarioStateAsync();
                    await CreateDialogContextFromScenarioAsync();
                    await currentTurn.SendActivityAsync(RandomResponses(AutenticazioneSuccessoResponse), cancellationToken: cancellationToken);
                    await SendMenuDipedentiHeroCardAsync();
                    return true;
                }
                else
                return false;
        }

        private async  Task SendMenuDipedentiHeroCardAsync()
        {
            var response = CreateResponse(currentTurn.Activity, CreateMenuDipdentiHeroCard());
            await currentTurn.SendActivityAsync(response, cancellationToken).ConfigureAwait(false);
        }

        private async Task SendMenuParkingHeroCard()
        {
            var response = CreateResponse(currentTurn.Activity, CreateMenuParkingHeroCard());
            await currentTurn.SendActivityAsync(response, cancellationToken).ConfigureAwait(false);
        }

        private Attachment CreateMenuDipdentiHeroCard()
        {
            var heroCard = new HeroCard()
            {
                Title = "Botler - Area Riservata -",
                Subtitle = "Il ChatBot di Reti S.p.A",
                Buttons = new List<CardAction> {

                    new CardAction(ActionTypes.PostBack, "Parcheggio Aziendale", value: Parking),
                    new CardAction(ActionTypes.PostBack, "QnA e F.A.Q Aziendale", value: "QnAInterno"),
                    new CardAction(ActionTypes.PostBack ,"News di Reti S.p.A", value: "News"),
                },
                Images = new List<CardImage> { new CardImage("https://i.pinimg.com/originals/0c/67/5a/0c675a8e1061478d2b7b21b330093444.gif")}

            };

            return heroCard.ToAttachment();
        }

       private Attachment CreateMenuParkingHeroCard()
        {
            var heroCard = new HeroCard()
            {
                Title = "Botler - Parcheggio Aziandale -",
                Subtitle = "Il ChatBot di Reti S.p.A",
                Buttons = new List<CardAction> {

                    new CardAction(ActionTypes.PostBack, "Prenota posto auto", value: PrenotazioneIntent),
                    new CardAction(ActionTypes.PostBack, "Visualizza tempo a disposizione", value: TempoRimanentePrenotazioneIntent),
                    new CardAction(ActionTypes.PostBack, "Visualizza Prenotazione", value: VerificaPrenotazioneIntent),
                    new CardAction(ActionTypes.PostBack, "Cancella Prenotazione", value: CancellaPrenotazioneIntent),

                },
                Images = new List<CardImage> { new CardImage("https://i.pinimg.com/originals/0c/67/5a/0c675a8e1061478d2b7b21b330093444.gif")}

            };

            return heroCard.ToAttachment();
        }

        private Activity CreateResponse(Activity activity, Attachment attachment)
        {
            var response = activity.CreateReply();
            response.Attachments = new List<Attachment>() { attachment };
            return response;
        }
    }
}