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

        // Dialog IDs
        private const string ProfileDialog = "profileDialog";

        // Initializes a new instance of the <see cref="PrenotazioneDialogo"/> class.
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
                    await UserProfileAccessor.SetAsync(stepContext.Context, new PrenotazioneModel());
                }
            }

            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> DisplayPrenotazioneStateStepAsync(
                                                    WaterfallStepContext stepContext,
                                                    CancellationToken cancellationToken)
        {
            var prenotazioneState = await UserProfileAccessor.GetAsync(stepContext.Context);
            var context = stepContext.Context;

            if (prenotazioneState != null && !string.IsNullOrWhiteSpace(prenotazioneState.scadenza.ToString()))
            {
                if (DateTime.Compare(DateTime.Now, DateTime.Parse(prenotazioneState.scadenza.ToString())) < 0)
                {
                    var countdown = ((Int32)DateTime.Parse(prenotazioneState.scadenza.ToString()).Subtract(DateTime.Now).TotalSeconds).ToString();
                    await context.SendActivityAsync($"La tua prenotazione è la seguente:\n-\t Nome lotto: {prenotazioneState.nomeLotto}\n-\t Tempo rimanente: {countdown}");
                    return await stepContext.EndDialogAsync();
                }
            }

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

            if (prenotazioneState != null && string.IsNullOrWhiteSpace(prenotazioneState.nomeLotto))
            {
                await context.SendActivityAsync("Non esiste alcuna prenotazione attiva!");
                return await stepContext.EndDialogAsync();
            }

            return await stepContext.EndDialogAsync();
        }
    }
}
