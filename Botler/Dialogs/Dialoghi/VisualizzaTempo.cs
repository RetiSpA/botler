using System;
using System.Threading;
using System.Threading.Tasks;
using Botler.Dialogs.RisorseApi;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Logging;

namespace Botler.Dialogs.Dialoghi
{
    public class VisualizzaTempo : ComponentDialog
    {
        // Dialog IDs
        private const string ProfileDialog = "profileDialog";

        // Inizializza una istanza della classe VisualizzaTempo.
        public VisualizzaTempo(IStatePropertyAccessor<PrenotazioneModel> userProfileStateAccessor, ILoggerFactory loggerFactory)
            : base(nameof(VisualizzaTempo))
        {
            UserProfileAccessor = userProfileStateAccessor ?? throw new ArgumentNullException(nameof(userProfileStateAccessor));

            // Add control flow dialogs
            var waterfallSteps = new WaterfallStep[]
            {
                    InitializeStateStepAsync,
                    DisplayScadenzaStateStepAsync,
            };
            AddDialog(new WaterfallDialog(ProfileDialog, waterfallSteps));
        }

        public IStatePropertyAccessor<PrenotazioneModel> UserProfileAccessor { get; }

        private async Task<DialogTurnResult> InitializeStateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var prenotazioneState = await UserProfileAccessor.GetAsync(stepContext.Context, () => null);
            if (prenotazioneState == null)
            {
                var prenotazioneStateOpt = stepContext.Options as PrenotazioneModel;
                if (prenotazioneStateOpt != null)
                {
                    await UserProfileAccessor.SetAsync(stepContext.Context, prenotazioneStateOpt);
                }
                else
                {
                    await UserProfileAccessor.SetAsync(stepContext.Context, new PrenotazioneModel(0, 0, 0, null, null, DateTime.MinValue, false, false));
                }
            }

            return await stepContext.NextAsync();
        }

        // Funzione che mostra la scadenza della prenotazione.
        private async Task<DialogTurnResult> DisplayScadenzaStateStepAsync(
                                                    WaterfallStepContext stepContext,
                                                    CancellationToken cancellationToken)
        {
            var prenotazioneState = await UserProfileAccessor.GetAsync(stepContext.Context);
            var context = stepContext.Context;

            // Se esiste una prenotazione attiva.
            if (prenotazioneState != null && !string.IsNullOrWhiteSpace(prenotazioneState.scadenza.ToString()))
            {
                if (DateTime.Compare(DateTime.Now, DateTime.Parse(prenotazioneState.scadenza.ToString())) < 0)
                {
                    var countdown = ((int)DateTime.Parse(prenotazioneState.scadenza.ToString()).Subtract(DateTime.Now).TotalSeconds).ToString();
                    await context.SendActivityAsync($"La tua prenotazione scade in {countdown} secondi");
                    return await stepContext.EndDialogAsync();
                }
            }

            // Se esiste una prenotazione scaduta, avvisa e cancella i dati memorizzati.
            if (prenotazioneState != null && !string.IsNullOrWhiteSpace(prenotazioneState.nomeLotto))
            {
                if (DateTime.Compare(DateTime.Now, DateTime.Parse(prenotazioneState.scadenza.ToString())) > 0)
                {
                    prenotazioneState.nomeLotto = null;
                    prenotazioneState.scadenza = DateTime.MinValue;
                    await context.SendActivityAsync($"La tua prenotazione è scaduta");
                    return await stepContext.EndDialogAsync();
                }
            }

            // Se non esiste alcuna prenotazione attiva.
            if (prenotazioneState != null && string.IsNullOrWhiteSpace(prenotazioneState.nomeLotto))
            {
                await context.SendActivityAsync("Non esiste alcuna prenotazione attiva!");
                return await stepContext.EndDialogAsync();
            }

            return await stepContext.EndDialogAsync();
        }
    }
}
