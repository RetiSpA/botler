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
            var context = stepContext.Context;
            PrenotazioneModel prenotazione = await Utility.Utility.getPrenotazione(Utility.Utility.bot_id);

            try
            {
                if (prenotazione != null)
                {
                    DateTime now = DateTime.Now;
                    TimeSpan differenza;
                    differenza = BasicBot.tempoPrenotazione.Subtract(now);
                    int minuti = (int)(differenza.TotalMinutes);
                    int secondi = (int)(differenza.TotalSeconds);

                    string[] responses = { "Il tuo parcheggio è ancora disponibile per" + minuti + " minuti e" + (secondi -(minuti * 60)) + " secondi",
                            "La disponibilità del tuo parcheggio scade tra" + minuti + " minuti e" + (secondi -(minuti * 60)) + " secondi",
                            "Hai ancora" + minuti + " minuti e" + (secondi -(minuti * 60)) + " secondi per usufruire del posteggio prenotato", };

                    Random rnd = new Random(); //crea new Random class
                    int i = rnd.Next(0, responses.Length);
                    await context.SendActivityAsync(responses[i]); //genera una risposta random

                    //await context.SendActivityAsync($"Il tuo parcheggio sarà disponibile ancora per: {minuti} minuti e {secondi - (minuti * 60)} secondi");
                    return await stepContext.EndDialogAsync();
                }
                else
                {
                    string[] responses = { "Non esiste alcuna prenotazione attiva!",
                        "Io non vedo nessuna prenotazione!",
                        "Ma quale prenotazione intendi scusa..",};

                    Random rnd = new Random();
                    int i = rnd.Next(0, responses.Length);
                    await context.SendActivityAsync(responses[i]); //genera una risposta random tra quelle presenti
                    return await stepContext.EndDialogAsync();
                }
            }
            catch
            {
                await context.SendActivityAsync($"Impossibile visualizzare la scadenza della prenotazione");
                return await stepContext.EndDialogAsync();
            }
        }
    }
}
