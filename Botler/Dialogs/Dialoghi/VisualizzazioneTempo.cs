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
    public class VisualizzazioneTempo : ComponentDialog
    {
        // User state for prenotazione dialog
        private const string PrenotazioneStateProperty = "prenotazioneState";
        private const string PrenotazioneValue = "prenotazioneName";

        // Dialog IDs
        private const string ProfileDialog = "profileDialog";

        // Initializes a new instance of the <see cref="PrenotazioneDialogo"/> class.
        public VisualizzazioneTempo(IStatePropertyAccessor<PrenotazioneModel> userProfileStateAccessor, ILoggerFactory loggerFactory)
            : base(nameof(VisualizzazioneTempo))
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
                    await UserProfileAccessor.SetAsync(stepContext.Context, new PrenotazioneModel());
                }
            }

            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> DisplayScadenzaStateStepAsync(
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
                prenotazioneState.scadenza = DateTime.Now;
            }
            return await stepContext.EndDialogAsync();
        }
    }
}
