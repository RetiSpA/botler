using System.Threading;
using System.Threading.Tasks;
using Botler.Middleware.Services;
using Microsoft.Bot.Builder;
using static Botler.Dialogs.Utility.BotConst;

namespace Botler.Builders
{
    public class LuisServiceResultBuilder
    {
        /// <summary>
        /// Create a Data structure that contains the luis result from the turn
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>LuisServiceResult-> [ReccognizeResult->intent,scoring; TopScoringIntent]</returns>
        public static async Task<LuisServiceResult> CreateLuisServiceResult(ITurnContext currentTurn, BotServices services, CancellationToken cancellationToken = default(CancellationToken))
        {
            var luisResult = await services.LuisServices[LuisConfiguration].RecognizeAsync(currentTurn, cancellationToken).ConfigureAwait(false);

            LuisServiceResult luisServiceResult = new LuisServiceResult(luisResult);

            return luisServiceResult;
        }
    }
}