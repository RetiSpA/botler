using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;
using Botler.Helper.Commands;
using Botler.Model;
using static Botler.Dialogs.Utility.BotConst;
using static Botler.Dialogs.Utility.Scenari;
using static Botler.Dialogs.Utility.Responses;
using static Botler.Dialogs.Utility.ListsResponsesIT;
using static Botler.Dialogs.Utility.Commands;


namespace Botler.Controller
{
    public class QnAController
    {
        public static async Task<bool> CheckQnAIsActive(BotlerAccessors accessors, ITurnContext turn)
        {
            var qna = await accessors.QnaActiveAccessors.GetAsync(turn, () => Default);

            if(!qna.Equals(Default))
            {
                return true;
            }
            return false;
        }

        public static async Task AnswerTurnUserQuestionAsync(ITurnContext turn, BotlerAccessors accessors, BotServices services)
        {
            QueryResult[] qnaResult = await GetQnAResult(turn, services, accessors);
            var qnaKey = await accessors.QnaActiveAccessors.GetAsync(turn, () => Default);

            await ShowQnAMessageAsync(qnaKey, turn, accessors);

            await SendQnAAnswerAsync(qnaResult, turn);
            await turn.SendActivityAsync(RandomResponses(ContinuaQnAResponse)).ConfigureAwait(false);

        }

        private async static Task ShowQnAMessageAsync(string qnaKey, ITurnContext turn, BotlerAccessors accessors)
        {
            if(qnaKey.Equals(QnAKey))
            {
                ICommand commandQnA = CommandFactory.FactoryMethod(turn, accessors, CommandQnARiservata);
                await commandQnA.ExecuteCommandAsync();
            }

            if(qnaKey.Equals(QnAPublicKey))
            {
                ICommand commandQnA = CommandFactory.FactoryMethod(turn, accessors, CommandQnAPublic);
                await commandQnA.ExecuteCommandAsync();
            }
        }

        private static async Task SendQnAAnswerAsync(QueryResult[] qnaResult, ITurnContext turn)
        {
            foreach(QueryResult result in qnaResult)
             {
                if(result.Score > 0.70)
                {
                // High Confidence Score
                var answer = result.Answer;
                await turn.SendActivityAsync(answer);
                }
            }
        }

        private static async Task<QueryResult[]> GetQnAResult(ITurnContext turn, BotServices services, BotlerAccessors accessors)
        {
            var currentQnA = await GetQnASelectedAsync(turn, accessors);
            return  await services.QnAServices[currentQnA].GetAnswersAsync(turn).ConfigureAwait(false);
        }

        private static async Task<string> GetQnASelectedAsync(ITurnContext turn, BotlerAccessors accessors)
        {
            return await accessors.QnaActiveAccessors.GetAsync(turn, () => Default);
        }
    }
}