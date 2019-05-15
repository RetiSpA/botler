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
    public class HelpCommand : ICommand
    {
        private readonly ITurnContext _turn;

        public HelpCommand(ITurnContext turn)
        {
            _turn = turn ?? throw new ArgumentNullException(nameof(turn));
        }
        public async Task ExecuteCommandAsync()
        {
            await _turn.SendActivityAsync(RandomResponses(PossibilitaResponse));
        }
    }
}