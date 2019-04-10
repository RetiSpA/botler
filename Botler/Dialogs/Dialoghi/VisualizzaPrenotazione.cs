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
                    if (DateTime.Compare(DateTime.Now, DateTime.Parse(Botler.tempoPrenotazione.ToString())) > 0)
                    {
                        var resp = await Utility.Utility.cancellaPrenotazione(prenotazione.id_posto);
                        if (resp)
                        {
                            string[] responses = { "La tua prenotazione è scaduta!",
                                        "E' terminato il tempo disponibile per il tuo posteggio",
                                        "La prenotazione è espirata", };
                            //rispsote possibili
                            Random rnd = new Random(); //crea new Random class
                            int i = rnd.Next(0, responses.Length);
                            await context.SendActivityAsync(responses[i]); //genera una risposta random
                            Botler.prenotazione = false;

                            return await stepContext.EndDialogAsync();
                        }
                    }
                    else
                    {
                        var prenotazioneNomeLotto = prenotazione.nomeLotto;
                        var prenotazioneIdPosto = prenotazione.id_posto;

                        string[] responses = { "La tua prenotazione? " + prenotazioneNomeLotto + ", " + prenotazioneIdPosto,
                        "Hai prenotato il parcheggio " + prenotazioneNomeLotto + " numero " + prenotazioneIdPosto,
                        prenotazioneNomeLotto + ", " + prenotazioneIdPosto + ", dovrebbe essere questa!",};

                        Random rnd = new Random(); //crea new Random class
                        int i = rnd.Next(0, responses.Length);
                        await context.SendActivityAsync(responses[i]); //genera una risposta random tra quelle presenti

                        return await stepContext.EndDialogAsync();
                    }

                }
                else
                // Nega l'esistenza di una prenotazione
                {
                    string[] responses = { "Non esiste alcuna prenotazione attiva!",
                        "Io non vedo nessuna prenotazione!",
                        "Ma quale prenotazione intendi scusa..",};

                    Random rnd = new Random();
                    int i = rnd.Next(0, responses.Length);
                    await context.SendActivityAsync(responses[i]); //genera una risposta random tra quelle presenti
                    return await stepContext.EndDialogAsync();
                }

                return await stepContext.EndDialogAsync();
            }
            catch
            {
                await context.SendActivityAsync($"Impossibile visualizzare la prenotazione");
                return await stepContext.EndDialogAsync();
            }
        }
    }
}
