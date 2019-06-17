using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;
using Botler.Commands;
using static Botler.Dialogs.Utility.BotConst;
using static Botler.Dialogs.Utility.Scenari;
using static Botler.Dialogs.Utility.Responses;
using static Botler.Dialogs.Utility.ListsResponsesIT;
using static Botler.Dialogs.Utility.Commands;
using Botler.Helpers;

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

        public static async Task<bool> AnsweredTurnUserQuestionAsync(ITurnContext turn, BotlerAccessors accessors, BotServices services)
        {
            string qnaKey = await AuthenticationHelper.GetQnAKeyFromAuthUser(turn, accessors);
            QueryResult[] qnaResult = await GetQnAResult(turn, services, qnaKey);
            string yesOrNo = turn.Activity.Text;

            if((yesOrNo.Equals("si") || yesOrNo.Equals("no")))
            {
                return false;
            }

            if(qnaResult.Length > 0)
            {
                if(qnaResult[0].Score > 0.80)
                {
                    await ShowQnAMessageAsync(qnaKey, turn, accessors);
                    await SendQnAAnswerAsync(qnaResult, turn);
                    await turn.SendActivityAsync(RandomResponses(ContinuaQnAResponse)).ConfigureAwait(false);

                    return true;
                }
            }
            return false;
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
                var answer = result.Answer;
                await turn.SendActivityAsync(answer);
            }
        }

        private static async Task<QueryResult[]> GetQnAResult(ITurnContext turn, BotServices services, string qnaKey)
        {
            return await services.QnAServices[qnaKey].GetAnswersAsync(turn).ConfigureAwait(false);
        }

        private static async Task<string> GetQnASelectedAsync(ITurnContext turn, BotlerAccessors accessors)
        {
            return await accessors.QnaActiveAccessors.GetAsync(turn, () => QnAPublicKey);
        }
    }
}