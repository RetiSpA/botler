using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using static Botler.Dialogs.Utility.Responses;
using static Botler.Dialogs.Utility.LuisIntent;
using static Botler.Dialogs.Utility.ListsResponsesIT;
using Botler.Services;

namespace Botler.Controller
{
    public class InterruptionRecognizer
    {
        public async static Task<bool> InterruptionHandledAsync(LuisServiceResult luisServiceResult, ITurnContext turn)
        {
            var topIntent = luisServiceResult.TopScoringIntent.Item1; // intent
            var score = luisServiceResult.TopScoringIntent.Item2; // score
            string response = string.Empty;

            if (topIntent.Equals(PresentazioneIntent) && (score > 0.75))
            {
                 response = RandomResponses(PresentazioneResponse);

                         // Handled the interrupt.
            }

            if (topIntent.Equals(GoodbyeIntent) && (score > 0.75))
            {
                 response= RandomResponses(SalutoResponse);

                         // Handled the interrupt.
            }

            if (topIntent.Equals(InformazioniIntent) && (score > 0.75))
            {
                 response = RandomResponses(InformazioneResponse);

                         // Handled the interrupt.
            }

            if (topIntent.Equals(RingraziamentiIntent) && (score > 0.75))
            {
                 response = RandomResponses(RingraziamentoResponse);

                         // Handled the interrupt.
            }

            if (topIntent.Equals(SalutePositivoIntent) && (score > 0.75))
            {
                 response = RandomResponses(SalutoPositivoResponse);

                         // Handled the interrupt.
            }

            if (topIntent.Equals(SaluteNegativoIntent) && (score > 0.75))
            {
                 response = RandomResponses(SalutoNegativoResponse);

                         // Handled the interrupt.
            }

            if (topIntent.Equals(AnomaliaIntent) && (score > 0.75))
            {
                 response = RandomResponses(AnomaliaResponse);

                         // Handled the interrupt.
            }

            if (topIntent.Equals(PossibilitàIntent) && (score > 0.75))
            {
                 response = RandomResponses(PossibilitaResponse);

                         // Handled the interrupt.
            }

            if(topIntent.Equals(RisataIntent) && (score > 0.75))
            {
                    response = RandomResponses(RisataResponse);
            }

                  if(response == string.Empty) return false;   // Did not handle

                  await turn.SendActivityAsync(response).ConfigureAwait(false);
                  return true;
        }
    }
}