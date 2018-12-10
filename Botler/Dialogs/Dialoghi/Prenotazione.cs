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

        // User state for prenotazione dialog
        private const string PrenotazioneStateProperty = "prenotazioneState";
        private const string PrenotazioneValue = "prenotazioneName";
        // Prompts names
        private const string NomeLottoPrompt = "nomeLottoPrompt";

        // Dialog IDs
        private const string ProfileDialog = "profileDialog";

        // Initializes a new instance of the <see cref="PrenotazioneDialogo"/> class.
        public Prenotazione(IStatePropertyAccessor<PrenotazioneModel> userProfileStateAccessor, ILoggerFactory loggerFactory)
            : base(nameof(Prenotazione))
        {
            UserProfileAccessor = userProfileStateAccessor ?? throw new ArgumentNullException(nameof(userProfileStateAccessor));

            // Add control flow dialogs
            var waterfallSteps = new WaterfallStep[]
            {
                    InitializeStateStepAsync,
                    PromptForPrenotazioneStepAsync,
                    SavePrenotazioneStateStepAsync,
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
                    await UserProfileAccessor.SetAsync(stepContext.Context, new PrenotazioneModel());
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

            //if (prenotazioneState != null && !string.IsNullOrWhiteSpace(prenotazioneState.nomeLotto))
            //{ if (DateTime.Compare(DateTime.Now, DateTime.Parse(prenotazioneState.scadenza.ToString())) > 0)
            //    {
            //        prenotazioneState.nomeLotto = null;
            //        prenotazioneState.scadenza = DateTime.MinValue;
            //        await context.SendActivityAsync($"La tua prenotazione è scaduta!");
            //        return await stepContext.EndDialogAsync();
            //    }
            //}

            //if (prenotazioneState != null && !string.IsNullOrWhiteSpace(prenotazioneState.nomeLotto))
            //{
            //    if (DateTime.Compare(DateTime.Now, DateTime.Parse(prenotazioneState.scadenza.ToString())) < 0) {
            //        var countdown = ((Int32)DateTime.Parse(prenotazioneState.scadenza.ToString()).Subtract(DateTime.Now).TotalSeconds).ToString();
            //        await context.SendActivityAsync($"La tua prenotazione è la seguente:\n-\t Nome lotto: {prenotazioneState.nomeLotto}\n-\t Tempo rimanente: {countdown}");
            //        return await stepContext.EndDialogAsync();
            //    }
            //}

            // Se non è già inserito io nome, richiede l'inserimento del nome.
            if (string.IsNullOrWhiteSpace(prenotazioneState.nomeLotto))
            {
                // richiesta del nome se mancante.
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

        // Funzione per mostrare la prenotazione all'utente.
        public async Task<DialogTurnResult> SavePrenotazioneStateStepAsync(
                                                    WaterfallStepContext stepContext,
                                                    CancellationToken cancellationToken)
        { 
            // Salva la prenotazione se inserita.
            var prenotazioneState = await UserProfileAccessor.GetAsync(stepContext.Context);

            var lowerCasePrenotazione = stepContext.Result as string;
            if (string.IsNullOrWhiteSpace(prenotazioneState.nomeLotto) &&
                !string.IsNullOrWhiteSpace(lowerCasePrenotazione))
            {
                // Mette l'iniziale della prenotazione maiuscola e la setta
                prenotazioneState.nomeLotto = char.ToUpper(lowerCasePrenotazione[0]) + lowerCasePrenotazione.Substring(1);
                await UserProfileAccessor.SetAsync(stepContext.Context, prenotazioneState);
                prenotazioneState.scadenza = DateTime.Now.AddSeconds(20);
            }
            return await PrenotazioneUser(stepContext);
        }

        // Funzione per mostrare all'utente la prenotazione effettuata.
        private async Task<DialogTurnResult> PrenotazioneUser(WaterfallStepContext stepContext)
        {
            var context = stepContext.Context;
            var prenotazioneState = await UserProfileAccessor.GetAsync(context);

            // Display their profile information and end dialog.
            await context.SendActivityAsync($"La tua prenotazione è stata confermata!");
            return await stepContext.EndDialogAsync();
        }
    }
}