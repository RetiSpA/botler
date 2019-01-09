using System;
using System.Threading;
using System.Threading.Tasks;
using Botler.Dialogs.RisorseApi;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace Botler.Dialogs.Dialoghi
{
    public class CancellaPrenotazione : ComponentDialog
    {
        // Prompts names
        private const string ConsensoPrompt = "consensoPrompt";

        // Dialog IDs
        private const string ProfileDialog = "profileDialog";

        // Inizializza una nuova istanza della classe CancellaPrenotazione.
        public CancellaPrenotazione(IStatePropertyAccessor<PrenotazioneModel> userProfileStateAccessor, ILoggerFactory loggerFactory)
            : base(nameof(CancellaPrenotazione))
        {
            UserProfileAccessor = userProfileStateAccessor ?? throw new ArgumentNullException(nameof(userProfileStateAccessor));

            // Add control flow dialogs
            var waterfallSteps = new WaterfallStep[]
            {
                    InitializeStateStepAsync,
                    PromptForCancellaPrenotazioneStepAsync,
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

        // Prompt per cancellare la prenotazione
        private async Task<DialogTurnResult> PromptForCancellaPrenotazioneStepAsync(
                                                WaterfallStepContext stepContext,
                                                CancellationToken cancellationToken)
        {
            var context = stepContext.Context;
            PrenotazioneModel prenotazione = await Utility.Utility.getPrenotazione(Utility.Utility.bot_id);

            try
            {
                if (prenotazione != null)
                {
                    if (DateTime.Compare(DateTime.Now, DateTime.Parse(BasicBot.tempoPrenotazione.ToString())) > 0)
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
                            BasicBot.prenotazione = false;
                            return await stepContext.EndDialogAsync();
                        }
                        else
                        {
                            BasicBot.prenotazione = false;
                            //rispsote possibili
                            string[] responses = { "Prenotazione cancellata con successo!",
                            "La prenotazione... Via! Andata! Caput!",
                            "Hai cestinato la tua prenotazione!", };
                            Random rnd = new Random(); //crea new Random class
                            int i = rnd.Next(0, responses.Length);
                            await context.SendActivityAsync(responses[i]); //genera una risposta random
                            return await stepContext.EndDialogAsync();
                        }
                    }
                    else
                    {
                        await context.SendActivityAsync($"Errore nel cancellare la prenotazione");
                        return await stepContext.EndDialogAsync();
                    }
                }
                else
                {
                    string[] responses = { "Non esiste alcuna prenotazione attiva!",
                               "Io non vedo nessuna prenotazione!",
                               "Ma quale prenotazione intendi scusa..", };
                    Random rnd = new Random(); //crea new Random class
                    int i = rnd.Next(0, responses.Length);
                    await context.SendActivityAsync(responses[i]); //genera una risposta random
                    return await stepContext.EndDialogAsync();
                }
            }
            catch
            {
                await context.SendActivityAsync($"Impossibile cancellare la prenotazione");
                return await stepContext.EndDialogAsync();
            }
        }
    }
}
