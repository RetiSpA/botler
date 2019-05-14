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
using Botler.Services;

namespace Botler.Dialogs.Scenari
{
    public class ParkingScenario : IScenario
    {
        private readonly BotlerAccessors _accessors;

        private readonly ITurnContext _turn;

        private readonly DialogSet _scenarioDialogs;

        public ParkingScenario(BotlerAccessors accessors, ITurnContext turn)
        {
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            _turn = turn ?? throw new ArgumentNullException(nameof(turn));

            ILoggerFactory loggerFactory = new LoggerFactory();
            _scenarioDialogs = new DialogSet(accessors.DialogStateAccessor);

            _scenarioDialogs.Add(new Prenotazione(_accessors.PrenotazioneStateAccessor, loggerFactory));
            _scenarioDialogs.Add(new CancellaPrenotazione(_accessors.CancellaPrenotazioneStateAccessor, loggerFactory));
            _scenarioDialogs.Add(new VisualizzaTempo(_accessors.VisualizzaTempoStateAccessor, loggerFactory));
            _scenarioDialogs.Add(new VisualizzaPrenotazione(_accessors.VisualizzaPrenotazioneStateAccessor, loggerFactory));
        }

        public async Task<DialogTurnResult> HandleDialogResultStatusAsync(LuisServiceResult luisServiceResult)
        {
            DialogContext currentDialogContext = await _scenarioDialogs.CreateContextAsync(_turn);
            var dialogResult = await currentDialogContext.ContinueDialogAsync();

            switch (dialogResult.Status)
                {
                    case DialogTurnStatus.Empty:
                    {
                        await _accessors.SetCurrentScenarioAsync(_turn, Parking);
                        return await StartDialog(luisServiceResult);
                    }

                    case DialogTurnStatus.Waiting:
                        await _accessors.SetCurrentScenarioAsync(_turn, Parking);
                        break;

                    case DialogTurnStatus.Complete:
                    {
                        await _accessors.SetCurrentScenarioAsync(_turn, Default);
                        return await currentDialogContext.EndDialogAsync();
                    }

                    default:
                    {
                        await _accessors.SetCurrentScenarioAsync(_turn, Default);
                        return await currentDialogContext.CancelAllDialogsAsync();
                    }
                }
            return null;
        }

        public bool NeedAuthentication()
        {
            return true;
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

            // Controlla autenticazione
            DialogTurnResult dialogResult = null;

            if(topIntent.Equals(PrenotazioneIntent) && score > 0.75)
            {
                dialogResult = await currentDialogContext.BeginDialogAsync(nameof(Prenotazione));

                return dialogResult;

            }

            if(topIntent.Equals(TempoRimanentePrenotazioneIntent) && score > 0.75)
            {

                dialogResult = await currentDialogContext.BeginDialogAsync(nameof(VisualizzaTempo));

                return dialogResult;
            }

            if(topIntent.Equals(CancellaPrenotazioneIntent) && score > 0.75)
            {
                dialogResult = await currentDialogContext.BeginDialogAsync(nameof(CancellaPrenotazione));

                 return dialogResult;

            }

            if(topIntent.Equals(VerificaPrenotazioneIntent) && score > 0.75)
            {
                dialogResult = await currentDialogContext.BeginDialogAsync(nameof(VisualizzaPrenotazione));

                return dialogResult;
            }

            else
            {
                await _turn.SendActivityAsync(RandomResponses(NoneResponse));
                return dialogResult;
            }
        }
    }
}