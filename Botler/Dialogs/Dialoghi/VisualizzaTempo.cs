using System;
using System.Threading;
using System.Threading.Tasks;
using Botler.Dialogs.RisorseApi;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Logging;
using Botler.Dialogs.Utility;

namespace Botler.Dialogs.Dialoghi
{
    public class VisualizzaTempo : ComponentDialog
    {
        // Dialog IDs
        private const string ProfileDialog = "profileDialog";
        private readonly Responses _responses;

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

            _responses = new Responses();
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
                    if (DateTime.Compare(DateTime.Now, DateTime.Parse(Botler.tempoPrenotazione.ToString())) > 0)
                    {
                        var resp = await Utility.Utility.cancellaPrenotazione(prenotazione.id_posto);
                        if (resp)
                        {
                            await context.SendActivityAsync(_responses.RandomResponses(_responses.PrenotazioneScadutaResponse)); //genera una risposta random
                            Botler.prenotazione = false;

                            return await stepContext.EndDialogAsync();
                        }
                    }
                    else
                    {
                        DateTime now = DateTime.Now;
                        TimeSpan differenza;
                        differenza = Botler.tempoPrenotazione.Subtract(now);

                        int minuti = (int)(differenza.TotalMinutes);
                        int secondi = (int)(differenza.TotalSeconds) - (minuti*60);
                        int ore = (int)(differenza.TotalHours);
                        int giorno = (int)(differenza.TotalDays);

                        string tempoPrenotazioneData = Botler.tempoPrenotazione.ToString("dd MMMM yyyy");
                        string tempoPrenotazioneOraInizio = Botler.tempoPrenotazione.AddHours(-1).ToString("HH:mm:ss");
                        string tempoPrenotazioneOraFine = Botler.tempoPrenotazione.ToString("HH:mm:ss");

                        // Invio data e ora della prenotazione, e tempo di validità
                        string randomRespDataOra = _responses.RandomResponses(_responses.PrenotazioneDataOraResponse);
                        await context.SendActivityAsync(String.Format(@randomRespDataOra, tempoPrenotazioneData, tempoPrenotazioneOraInizio, tempoPrenotazioneOraFine));
                        // Invio del tempo a disposizione, prima che il parcheggio scada
                        string randomRespTempoDisp = _responses.RandomResponses(_responses.PrenotazioneTempoDisponibileResponse);
                        await context.SendActivityAsync(String.Format(@randomRespTempoDisp, minuti, secondi));

                        return await stepContext.EndDialogAsync();
                    }
                    return await stepContext.EndDialogAsync();
                }
                else
                {
                    await context.SendActivityAsync(_responses.RandomResponses(_responses.PrenotazioneNonTrovataResponse));
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
