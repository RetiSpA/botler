using System;
using System.Threading.Tasks;
using Botler.Controller;
using Botler.Model;
using Microsoft.Bot.Builder;
using static Botler.Dialogs.Utility.BotConst;
using static Botler.Dialogs.Utility.ListsResponsesIT;
using static Botler.Dialogs.Utility.Responses;
using static Botler.Dialogs.Utility.Scenari;

namespace Botler.Helper.Commands
{
    public class LogoutCommand : ICommand
    {
        private readonly ITurnContext _turn;

        private readonly BotlerAccessors _accessors;

        public LogoutCommand(ITurnContext turn, BotlerAccessors accessors)
        {
            this._turn = turn ?? throw new ArgumentNullException(nameof(turn));
            this._accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
        }

        public async Task ExecuteCommandAsync()
        {
            var alreadyAuth = await Autenticatore.UserAlreadyAuthAsync(_turn, _accessors);

            if(alreadyAuth)
            {

            var adapter = (BotFrameworkAdapter)_turn.Adapter;
            await adapter.SignOutUserAsync(_turn, ConnectionName);
            await _accessors.AutenticazioneDipedenteAccessors.SetAsync(_turn, false);

            if(_accessors.ActiveScenario != null)
            {
                 var dialogContext = await _accessors.ActiveScenario.GetDialogContextAsync();
                 await dialogContext.CancelAllDialogsAsync();
            }

            await _accessors.ResetScenarioAsync(_turn);

            await _turn.SendActivityAsync(RandomResponses(LogoutEffettuatoResponse));
            }

            else
            {
                await _turn.SendActivityAsync(RandomResponses(LogoutImpossibileResponse));
            }
        }
    }
}