using System;
using System.Threading;
using System.Threading.Tasks;
using Botler.Dialogs.RisorseApi;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Logging;

namespace Botler.Dialogs.Dialoghi
{
    public class Prenotazione : ComponentDialog
    {
        // Dialog IDs
        private const string ProfileDialog = "profileDialog";
        private static int timeoutParcheggio = 7;

        // Inizializza una nuova istanza della classe Prenotazione.
        public Prenotazione(IStatePropertyAccessor<PrenotazioneModel> userProfileStateAccessor, ILoggerFactory loggerFactory)
            : base(nameof(Prenotazione))
        {
            UserProfileAccessor = userProfileStateAccessor ?? throw new ArgumentNullException(nameof(userProfileStateAccessor));
            // Add control flow dialogs
            var waterfallSteps = new WaterfallStep[]
            {
                    InitializeStateStepAsync,
                    PromptForPrenotazioneStepAsync,
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

        private async Task<DialogTurnResult> PromptForPrenotazioneStepAsync(
                                                WaterfallStepContext stepContext,
                                                CancellationToken cancellationToken)
        {
            var context = stepContext.Context;

            try
            {
                PosteggioModel posto = await Utility.Utility.setPosteggioAutoassegnato(Utility.Utility.lotto_id);
                if (posto != null)
                {
                    PrenotazioneModel prenotazione = await Utility.Utility.prenotaPosteggio(Utility.Utility.lotto_id, posto.id_posteggio);
                    if (prenotazione != null)
                    {
                        BasicBot.tempoPrenotazione = DateTime.Now;
                        BasicBot.tempoPrenotazione = BasicBot.tempoPrenotazione.AddMinutes(timeoutParcheggio);
                        BasicBot.prenotazione = true;
                        await context.SendActivityAsync($"Prenotazione effettuata in: {prenotazione.nomeLotto }, parcheggio: {prenotazione.id_posto}");
                        return await stepContext.EndDialogAsync();
                    }
                    else
                    {
                        PrenotazioneModel verificaPrenotazione = await Utility.Utility.getPrenotazione(Utility.Utility.bot_id);
                        if (verificaPrenotazione != null)
                        {
                            await context.SendActivityAsync($"Tutti i posti sono occupati!");
                            return await stepContext.EndDialogAsync();
                        }
                        else
                        {
                            await context.SendActivityAsync($"Possiedi già una prenotazione!");
                            return await stepContext.EndDialogAsync();
                        }
                    }
                }

                return await stepContext.EndDialogAsync();
            }
            catch
            {
                await context.SendActivityAsync($"Impossibile cancellare la prenotazione!");
                return await stepContext.EndDialogAsync();
            }
        }
    }
}

// Se esiste una prenotazione scaduta, avvisa e cancella i dati memorizzati.
//if (prenotazioneState != null && !string.IsNullOrWhiteSpace(prenotazioneState.nomeLotto))
//{
//    if (DateTime.Compare(DateTime.Now, DateTime.Parse(prenotazioneState.scadenza.ToString())) > 0)
//    {
//        prenotazioneState.nomeLotto = null;
//        prenotazioneState.scadenza = DateTime.MinValue;
//        await context.SendActivityAsync($"La tua prenotazione è scaduta!");
//        return await stepContext.EndDialogAsync();
//    }
//}
//return await stepContext.EndDialogAsync();
