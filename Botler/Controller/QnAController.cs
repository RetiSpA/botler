using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;
using static Botler.Dialogs.Utility.BotConst;
using static Botler.Dialogs.Utility.Scenari;
namespace Botler.Controller
{
    public class QnAController
    {
        public static async Task<bool> CheckQnAIsActive(BotlerAccessors accessors, ITurnContext turn)
        {
            var qna = await accessors.QnaActiveAccessors.GetAsync(turn, () => Default);
            Console.WriteLine(qna.ToString());
            if(!qna.Equals(Default))
            {
                return true;
            }
            return false;
        }

        public static async Task AnswerTurnUserQuestionAsync(ITurnContext turn, BotlerAccessors accessors, BotServices services)
        {
            QueryResult[] qnaResult = await GetQnAResult(turn, services, accessors);
            await SendQnAAnswerAsync(qnaResult, turn);

        }

        private static async Task SendQnAAnswerAsync(QueryResult[] qnaResult, ITurnContext turn)
        {
            foreach(QueryResult result in qnaResult)
             {
                var answer = result.Answer;
                await turn.SendActivityAsync(answer);
             }
        }

        private static async Task<QueryResult[]> GetQnAResult(ITurnContext turn, BotServices services, BotlerAccessors accessors)
        {
            var currentQnA = await GetQnASelectedAsync(turn, accessors);
            return await services.QnAServices[currentQnA].GetAnswersAsync(turn).ConfigureAwait(false);
        }

        private static async Task<string> GetQnASelectedAsync(ITurnContext turn, BotlerAccessors accessors)
        {
            return await accessors.QnaActiveAccessors.GetAsync(turn, () => Default);
        }
    }
}