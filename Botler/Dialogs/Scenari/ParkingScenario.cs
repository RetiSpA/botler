using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Botler.Dialogs.Dialoghi;
using Botler.Dialogs.RisorseApi;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Botler.Dialogs.Utility;
using Microsoft.Extensions.Logging;
using Microsoft.Bot.Schema;
using Botler.Controller;
using static Botler.Dialogs.Utility.Scenari;
using static Botler.Dialogs.Utility.BotConst;
using static Botler.Dialogs.Utility.LuisIntent;
using static Botler.Dialogs.Utility.ListsResponsesIT;
using static Botler.Dialogs.Utility.Responses;
using Botler.Middleware.Services;
using Botler.Models;
using Botler.Controllers;

namespace Botler.Dialogs.Scenari
{
    public class ParkingScenario : ExecutionScenario
    {
        private readonly BotlerAccessors _accessors;

        private readonly ITurnContext _turn;

        private readonly DialogSet _scenarioDialogs;

        public override string ScenarioID { get; set; } = Parking;

        public override Intent ScenarioIntent { get; set; }

        public override bool NeedAuthentication { get; set; } = true;

        public override string AssociatedScenario { get; set; } = Parking;

        public ParkingScenario(BotlerAccessors accessors, ITurnContext turn)
        {
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            _turn = turn ?? throw new ArgumentNullException(nameof(turn));
            ILoggerFactory loggerFactory = new LoggerFactory();
            _scenarioDialogs = new DialogSet(_accessors.DialogStateAccessor);

            _scenarioDialogs.Add(new Prenotazione(_accessors, loggerFactory));
            _scenarioDialogs.Add(new CancellaPrenotazione(_accessors.CancellaPrenotazioneStateAccessor, loggerFactory));
            _scenarioDialogs.Add(new VisualizzaTempo(_accessors.VisualizzaTempoStateAccessor, loggerFactory));
            _scenarioDialogs.Add(new VisualizzaPrenotazione(_accessors.VisualizzaPrenotazioneStateAccessor, loggerFactory));
        }

        public override async Task CreateResponseAsync(LuisServiceResult luisServiceResult)
        {
            DialogContext currentDialogContext = await _scenarioDialogs.CreateContextAsync(_turn);
            var dialogResult = await currentDialogContext.ContinueDialogAsync();

            switch (dialogResult.Status)
                {
                    case DialogTurnStatus.Empty:
                    {
                        await _accessors.SetCurrentScenarioAsync(_turn, Parking);
                        await StartDialog(luisServiceResult);
                        break;
                    }

                    case DialogTurnStatus.Waiting:
                        await _accessors.SetCurrentScenarioAsync(_turn, Parking);
                        break;

                    case DialogTurnStatus.Complete:
                    {
                        await _accessors.SetCurrentScenarioAsync(_turn, Default);
                        await currentDialogContext.EndDialogAsync();
                        break;
                    }

                    default:
                    {
                        await _accessors.SetCurrentScenarioAsync(_turn, Default);
                        await currentDialogContext.CancelAllDialogsAsync();
                        break;
                    }
                }
        }

        public async  Task<DialogContext> GetDialogContextAsync()
        {
            DialogContext currentDialogContext = await _scenarioDialogs.CreateContextAsync(_turn);
            return currentDialogContext;
        }

        private async Task<DialogTurnResult> StartDialog(LuisServiceResult luisServiceResult)
        {
            var topIntent = luisServiceResult.TopScoringIntent.Item1; // intent
            var score = luisServiceResult.TopScoringIntent.Item2; // score
            DialogContext currentDialogContext = await _scenarioDialogs.CreateContextAsync(_turn);

            DialogTurnResult dialogResult = null;

            if (topIntent.Equals(PrenotazioneParcheggioIntent) && score >= 0.75)
            {
                dialogResult = await currentDialogContext.BeginDialogAsync(nameof(Prenotazione));
                return dialogResult;
            }

            if(topIntent.Equals(TempoRimanentePrenotazioneParcheggioIntent) && score >= 0.75)
            {

                dialogResult = await currentDialogContext.BeginDialogAsync(nameof(VisualizzaTempo));
                return dialogResult;
            }

            if(topIntent.Equals(CancellaPrenotazioneParcheggioIntent) && score >= 0.75)
            {
                dialogResult = await currentDialogContext.BeginDialogAsync(nameof(CancellaPrenotazione));
                return dialogResult;
            }

            if(topIntent.Equals(VerificaPrenotazioneParcheggioIntent) && score >= 0.75)
            {
                dialogResult = await currentDialogContext.BeginDialogAsync(nameof(VisualizzaPrenotazione));
                return dialogResult;
            }

            await _turn.SendActivityAsync(RandomResponses(NoneResponse) + "\n" + RandomResponses(PossibilitaResponse));
            return dialogResult;

        }
    }
}