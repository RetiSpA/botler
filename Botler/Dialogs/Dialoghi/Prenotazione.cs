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
                        //BasicBot.tempoPrenotazione = DateTime.Now;
                        BasicBot.tempoPrenotazione = DateTime.Now.AddSeconds(10);
                        BasicBot.prenotazione = true;
                        var prenotazioneNomeLotto = prenotazione.nomeLotto;
                        var prenotazioneIdPosto = prenotazione.id_posto;

                        //risposte possibili
                        string[] responses = { "Hai effettuato una pranotazione in " + prenotazioneNomeLotto + " nel parcheggio " + prenotazioneIdPosto,
                            "La tua prenotazione è la seguente: " + prenotazioneNomeLotto + ", " + prenotazioneIdPosto,
                            "Il posteggio in " + prenotazioneNomeLotto + " , numero " + prenotazioneIdPosto + " ti è stato assegnato", };

                        Random rnd = new Random(); //crea new Random class
                        int i = rnd.Next(0, responses.Length);
                        await context.SendActivityAsync(responses[i]); //genera una risposta random
                        return await stepContext.EndDialogAsync();
                    }
                    else
                    {
                        PrenotazioneModel verificaPrenotazione = await Utility.Utility.getPrenotazione(Utility.Utility.bot_id);
                        if (verificaPrenotazione != null)
                        {
                            if (DateTime.Compare(DateTime.Now, DateTime.Parse(BasicBot.tempoPrenotazione.ToString())) > 0)
                            {
                                var resp = await Utility.Utility.cancellaPrenotazione(verificaPrenotazione.id_posto);
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
                            }
                            else
                            {
                                string[] responses = { "Possiedi già una prenotazione!",
                                "Puoi avere solo un posto!",
                                "Ma quanti posti vuoi oh! Ne hai già uno!", };
                                //rispsote possibili
                                Random rnd = new Random(); //crea new Random class
                                int i = rnd.Next(0, responses.Length);
                                await context.SendActivityAsync(responses[i]); //genera una risposta random
                                return await stepContext.EndDialogAsync();
                            }
                        }
                        //else
                        //{
                        //    string[] responses = { "Tutti i posti sono occupati!",
                        //        "Non ci sono più posti!",
                        //        "Vuoi prenotare un posto eh?! Beh son finiti!", };  //rispsote possibili
                        //    Random rnd = new Random(); //crea new Random class
                        //    int i = rnd.Next(0, responses.Length);
                        //    await context.SendActivityAsync(responses[i]); //genera una risposta random
                        //    return await stepContext.EndDialogAsync();
                        //}
                    }
                }

                return await stepContext.EndDialogAsync();
            }
            catch
            {
                await context.SendActivityAsync($"Impossibile visualizzare la prenotazione!");
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
