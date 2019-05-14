using System;
using System.Threading.Tasks;
using Botler.Controller;
using Botler.Model;
using Microsoft.Bot.Builder;
using static Botler.Dialogs.Utility.Scenari;
using static Botler.Dialogs.Utility.ListsResponsesIT;
using static Botler.Dialogs.Utility.Responses;
using static Botler.Dialogs.Utility.BotConst;
using static Botler.Dialogs.Utility.Commands;

namespace Botler.Helper.Commands
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
            var alreadyAuth =  await Autenticatore.UserAlreadyAuthAsync(_turn, _accessors);

            if(alreadyAuth)
            {
                await _accessors.QnaActiveAccessors.SetAsync(_turn, QnAKey);
                ICommand commandQnARiservata = CommandFactory.FactoryMethod(_turn, _accessors, CommandQnARiservata);
                await commandQnARiservata.ExecuteCommandAsync();
            }

            else
            {
                await _accessors.QnaActiveAccessors.SetAsync(_turn, QnAPublicKey);
                ICommand commandQnAPublic = CommandFactory.FactoryMethod(_turn, _accessors, CommandQnAPublic);
                await commandQnAPublic.ExecuteCommandAsync();
            }

        }
    }
}