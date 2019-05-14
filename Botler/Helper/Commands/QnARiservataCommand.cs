using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using static Botler.Dialogs.Utility.Scenari;
using static Botler.Dialogs.Utility.ListsResponsesIT;
using static Botler.Dialogs.Utility.Responses;
using static Botler.Dialogs.Utility.BotConst;
using static Botler.Dialogs.Utility.Commands;
using Botler.Model;

namespace Botler.Helper.Commands
{
    public class QnARiservataCommand : ICommand
    {
        private readonly BotlerAccessors _accessors;

        private readonly ITurnContext _turn;

        public QnARiservataCommand(BotlerAccessors accessors, ITurnContext turn)
        {
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            _turn = turn ?? throw new ArgumentNullException(nameof(turn));
        }

        public async Task ExecuteCommandAsync()
        {
           await SetScenarioQnAAsync();
           await _turn.SendActivityAsync(RandomResponses(QnAResponse));
        }

        private async Task SetScenarioQnAAsync()
        {
            await _accessors.QnaActiveAccessors.SetAsync(_turn, QnAKey);
            await _accessors.SetCurrentScenarioAsync(_turn, QnAPublic);
        }
    }
}