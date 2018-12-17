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
    public class Prenotazione : ComponentDialog
    {
        // Prompts names
        private const string NomeLottoPrompt = "nomeLottoPrompt";

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
                    SalvaPrenotazioneStateStepAsync,
            };
            AddDialog(new WaterfallDialog(ProfileDialog, waterfallSteps));
            AddDialog(new TextPrompt(NomeLottoPrompt));
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
            var prenotazioneState = await UserProfileAccessor.GetAsync(context);


            if (prenotazioneState != null && !string.IsNullOrWhiteSpace(prenotazioneState.nomeLotto))
            {
                if (DateTime.Compare(DateTime.Now, DateTime.Parse(prenotazioneState.scadenza.ToString())) < 0)
                {
                    await context.SendActivityAsync($"Possiedi già una prenotazione attiva!");
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
                    await context.SendActivityAsync($"La tua prenotazione è scaduta!");
                    return await stepContext.EndDialogAsync();
                }
            }

            // Se non è presente una prenotazione, richiede l'inserimento del nome per effettuarla.
            if (string.IsNullOrWhiteSpace(prenotazioneState.nomeLotto))
            {
                var opts = new PromptOptions
                {
                    Prompt = new Activity
                    {
                        Type = ActivityTypes.Message,
                        Text = "Quale posteggio vuoi prenotare?",
                    },
                };
                return await stepContext.PromptAsync(NomeLottoPrompt, opts);
            }
            else
            {
                return await stepContext.NextAsync();
            }
        }

        // Funzione per salvare la prenotazione dell'utente.
        public async Task<DialogTurnResult> SalvaPrenotazioneStateStepAsync(
                                                    WaterfallStepContext stepContext,
                                                    CancellationToken cancellationToken)
        {
            // Salva la prenotazione inserita.
            var prenotazioneState = await UserProfileAccessor.GetAsync(stepContext.Context);
            var lowerCasePrenotazione = stepContext.Result as string;

            if (string.IsNullOrWhiteSpace(prenotazioneState.nomeLotto) && !string.IsNullOrWhiteSpace(lowerCasePrenotazione))
            {
                // Mette l'iniziale della prenotazione maiuscola e la setta.
                prenotazioneState.nomeLotto = char.ToUpper(lowerCasePrenotazione[0]) + lowerCasePrenotazione.Substring(1);
                await UserProfileAccessor.SetAsync(stepContext.Context, prenotazioneState);
                prenotazioneState.scadenza = DateTime.Now.AddSeconds(20);
            }

            return await PrenotazioneUser(stepContext);
        }

        // Funzione per confermare l'avvenuta prenotazione precedente.
        private async Task<DialogTurnResult> PrenotazioneUser(WaterfallStepContext stepContext)
        {
            var context = stepContext.Context;
            var prenotazioneState = await UserProfileAccessor.GetAsync(context);
            await context.SendActivityAsync($"La tua prenotazione è stata confermata!");
            return await stepContext.EndDialogAsync();
        }
    }
}