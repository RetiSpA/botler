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
        // User state for prenotazione dialog
        private const string PrenotazioneStateProperty = "prenotazioneState";
        private const string PrenotazioneValue = "prenotazioneName";

        // Prompts names
        private const string ConsensoPrompt = "consensoPrompt";

        // Dialog IDs
        private const string ProfileDialog = "profileDialog";

        // Initializes a new instance of the <see cref="PrenotazioneDialogo"/> class.
        public CancellaPrenotazione(IStatePropertyAccessor<PrenotazioneModel> userProfileStateAccessor, ILoggerFactory loggerFactory)
            : base(nameof(CancellaPrenotazione))
        {
            UserProfileAccessor = userProfileStateAccessor ?? throw new ArgumentNullException(nameof(userProfileStateAccessor));

            // Add control flow dialogs
            var waterfallSteps = new WaterfallStep[]
            {
                    InitializeStateStepAsync,
                    PromptForCancellaPrenotazioneStepAsync,
                    CancelPrenotazioneStepAsync,
            };
            AddDialog(new WaterfallDialog(ProfileDialog, waterfallSteps));
            AddDialog(new TextPrompt(ConsensoPrompt));
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

        private async Task<DialogTurnResult> PromptForCancellaPrenotazioneStepAsync(
                                                WaterfallStepContext stepContext,
                                                CancellationToken cancellationToken)
        {
            var context = stepContext.Context;
            var prenotazioneState = await UserProfileAccessor.GetAsync(context);

            if (prenotazioneState != null && !string.IsNullOrWhiteSpace(prenotazioneState.nomeLotto))
            {
                // richiesta del nome se mancante.
                var opts = new PromptOptions
                {
                    Prompt = new Activity
                    {
                        Type = ActivityTypes.Message,
                        Text = "Vuoi eliminare la tua prenotazione??",
                    },
                };
                return await stepContext.PromptAsync(ConsensoPrompt, opts);
            }
            else
            {
                await context.SendActivityAsync($"Non è presente alcuna prenotazione attiva!");
                return await stepContext.EndDialogAsync();
            }
        }

        private async Task<DialogTurnResult> CancelPrenotazioneStepAsync(
                                                    WaterfallStepContext stepContext,
                                                    CancellationToken cancellationToken)
        {
            var prenotazioneState = await UserProfileAccessor.GetAsync(stepContext.Context);
            string consenso = (string)stepContext.Result;
            if (consenso.Equals("si") || consenso.Equals("certo") || consenso.Equals("ok"))
            {
                prenotazioneState.nomeLotto = null;
                await UserProfileAccessor.SetAsync(stepContext.Context, prenotazioneState);
                await stepContext.Context.SendActivityAsync($"La tua prenotazione è stata rimossa con successo!");
            }
            else
            {
                await stepContext.Context.SendActivityAsync($"Se non sei sicuro non farmi perdere tempo!");
            }

            return await stepContext.EndDialogAsync();
        }
    }
}
