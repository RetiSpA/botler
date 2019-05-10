using System;
using System.Threading.Tasks;
using Botler.Model;
using Microsoft.Bot.Builder;
using static Botler.Dialogs.Utility.Scenari;
using static Botler.Dialogs.Utility.ListsResponsesIT;
using static Botler.Dialogs.Utility.Responses;
using static Botler.Dialogs.Utility.BotConst;
namespace Botler.Helper.Commands
{
    public class QnAPublicCommand : ICommand
    {
        private readonly BotlerAccessors _accessors;

        private readonly ITurnContext _turn;

        public QnAPublicCommand(BotlerAccessors accessors, ITurnContext turn)
        {
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            _turn = turn ?? throw new ArgumentNullException(nameof(turn));
        }
        public async Task ExecuteCommandAsync()
        {
           await SetScenarioQnAAsync();
           await _accessors.SaveStateAsync(_turn);
           await _turn.SendActivityAsync(RandomResponses(QnAPublicResponse));
           await SendRandomQnAAsync();
        }

        private async Task SetScenarioQnAAsync()
        {
            await _accessors.QnaActiveAccessors.SetAsync(_turn, QnAPublicKey);
            await _accessors.ScenarioStateAccessors.SetAsync(_turn, QnAPublic);
        }

        private async Task SendRandomQnAAsync()
        {
            for(int i = 0; i<3; i++)
            {
                await _turn.SendActivityAsync(RandomResponses(DomandaResponse));
            }
        }

    }
}