using System;
using System.Threading.Tasks;
using Botler.Controller;
using Botler.Models;
using Microsoft.Bot.Builder;
using static Botler.Dialogs.Utility.Scenari;
using static Botler.Dialogs.Utility.ListsResponsesIT;
using static Botler.Dialogs.Utility.Responses;
using static Botler.Dialogs.Utility.BotConst;
using static Botler.Dialogs.Utility.Commands;

namespace Botler.Commands
{
    public class QnACommand : ICommand
    {
        private readonly BotlerAccessors _accessors;

        private readonly ITurnContext _turn;

        public QnACommand(BotlerAccessors accessors, ITurnContext turn)
        {
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            _turn = turn ?? throw new ArgumentNullException(nameof(turn));
        }

        public async Task ExecuteCommandAsync()
        {
            var alreadyAuth = await AuthenticationHelper.UserAlreadyAuthAsync(_turn, _accessors);

            if (alreadyAuth)
            {
               await ExecuteCommandQnAAsync(QnAKey, CommandQnARiservata);
            }
            else
            {
               await ExecuteCommandQnAAsync(QnAPublicKey, CommandQnAPublic);
            }
        }

        private async Task ExecuteCommandQnAAsync(string qnaKey, string qnaCommand)
        {
            await _accessors.QnaActiveAccessors.SetAsync(_turn, qnaKey);
            ICommand commandQna = CommandFactory.FactoryMethod(_turn, _accessors, qnaCommand);
            await commandQna.ExecuteCommandAsync();
        }
    }
}