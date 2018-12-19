using System;
using System.Threading;
using System.Threading.Tasks;
using Botler.Dialogs.RisorseApi;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Logging;

namespace Botler.Dialogs.Dialoghi
{
    public class VisualizzaPrenotazione : ComponentDialog
    {
        // Dialog IDs.
        private const string ProfileDialog = "profileDialog";

        // Inizializza una nuova istanza della classe Visualizza prenotazione.
        public VisualizzaPrenotazione(IStatePropertyAccessor<PrenotazioneModel> userProfileStateAccessor, ILoggerFactory loggerFactory)
            : base(nameof(VisualizzaPrenotazione))
        {
            UserProfileAccessor = userProfileStateAccessor ?? throw new ArgumentNullException(nameof(userProfileStateAccessor));

            // Add control flow dialogs
            var waterfallSteps = new WaterfallStep[]
            {
                    InitializeStateStepAsync,
                    DisplayPrenotazioneStateStepAsync,
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

        // Funzione per mostrare la prenotazione.
        private async Task<DialogTurnResult> DisplayPrenotazioneStateStepAsync(
                                                    WaterfallStepContext stepContext,
                                                    CancellationToken cancellationToken)
        {
            var context = stepContext.Context;
            PrenotazioneModel prenotazione = await Utility.Utility.getPrenotazione(Utility.Utility.bot_id);
            try
            {
                // Mostra la prenotazione se attiva.
                if (prenotazione != null)
                {
                    await context.SendActivityAsync($"Hai prenotato nel lotto: { prenotazione.nomeLotto}, il parcheggio: { prenotazione.id_posto}");
                    return await stepContext.EndDialogAsync();
                }
                else
                {
                    await context.SendActivityAsync($"Nessuna prenotazione esistente!");
                    return await stepContext.EndDialogAsync();
                }
            }
            catch
            {
                await context.SendActivityAsync($"Impossibile visualizzare la prenotazione");
                return await stepContext.EndDialogAsync();
            }
        }
    }
}
