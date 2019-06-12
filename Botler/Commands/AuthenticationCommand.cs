
using Botler.Models;
using Botler.Controller;
using Botler.Commands;
using Botler.Dialogs;
using Botler.Dialogs.RisorseApi;
using Microsoft.Graph;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using static Botler.Dialogs.Utility.Commands;
using static Botler.Dialogs.Utility.Scenari;
using static Botler.Dialogs.Utility.Responses;
using static Botler.Dialogs.Utility.ListsResponsesIT;
using static Botler.Dialogs.Utility.BotConst;
using Botler.Helpers;
using Botler.Builders;

namespace Botler.Commands
{
    public class AuthenticationCommand : ICommand
    {
        private readonly ITurnContext _turn;

        private readonly BotlerAccessors _accessors;

        public AuthenticationCommand(ITurnContext turn, BotlerAccessors accessors)
        {
            this._turn = turn ?? throw new ArgumentNullException(nameof(turn));
            this._accessors = accessors ?? throw new ArgumentNullException(nameof(_accessors));

        }

        public async Task ExecuteCommandAsync()
        {

            var message = _turn.Activity.Text;
            var alreadyAuth = await AuthenticationHelper.UserAlreadyAuthAsync(_turn, _accessors);

            if (alreadyAuth)
            {
                await ChangesAndSaveStateAsync();
                await ShowAreaRiservataAsync();
                return;
            }

            if (AuthenticationHelper.MagicCodeFound(message))
            {
                await AuthenticationHelper.SecondPhaseAuthAsync(_turn, _accessors);
            }
            else
            {
                await AuthenticationHelper.FirstPhaseAuthAsync(_turn, _accessors);
            }
        }


        private async Task ChangesAndSaveStateAsync()
        {
            await _accessors.SetCurrentScenarioAsync(_turn, Default);
            await _accessors.SaveStateAsync(_turn);
        }

        private async Task ShowAreaRiservataAsync()
        {
            // await _turn.SendActivityAsync(RandomResponses(AutenticazioneEffettuataResponse));
            ICommand commandAreaRiservata = CommandFactory.FactoryMethod(_turn, _accessors, CommandAreaRiservata);
            await commandAreaRiservata.ExecuteCommandAsync();
        }
    }
}