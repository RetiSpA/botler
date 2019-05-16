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
    public class ChangeLanguageEnCommang : ICommand
    {
        private readonly ITurnContext _turn;

        private readonly BotlerAccessors _accessors;

        public ChangeLanguageEnCommang(ITurnContext turn, BotlerAccessors accessors)
        {
            _turn = turn ?? throw new ArgumentNullException(nameof(turn));
            _accessors = accessors  ?? throw new ArgumentNullException(nameof(accessors));
        }
        public async Task ExecuteCommandAsync()
        {
            await _accessors.ResourceFileSelectedAccessors.SetAsync(_turn, ResourceEN);
        }
    }
}