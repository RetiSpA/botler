using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Botler.Dialogs.RisorseApi;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Botler.Controller;
using Botler.Dialogs.Utility;
using Botler.Helper.Commands;
using Botler.Model;
using Microsoft.Extensions.Logging;
using static Botler.Dialogs.Utility.Responses;
using static Botler.Dialogs.Utility.Commands;
using static Botler.Dialogs.Utility.ListsResponsesIT;
using Botler.Services;

namespace Botler.Dialogs.Scenari
{
    public class AutenticazioneScenario : IScenario
    {
        private readonly DialogSet _scenarioDialogs;

        private readonly ITurnContext _turn;

        private readonly BotlerAccessors _accessors;

        public AutenticazioneScenario(BotlerAccessors accessors, ITurnContext turn )
        {
            _scenarioDialogs = new DialogSet(accessors.DialogStateAccessor);
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            _turn = turn ?? throw new ArgumentNullException(nameof(turn));
        }

        public async  Task<DialogContext> GetDialogContextAsync()
        {
            DialogContext currentDialogContext = await _scenarioDialogs.CreateContextAsync(_turn);
            return currentDialogContext;
        }

        public async Task<DialogTurnResult> HandleDialogResultStatusAsync(LuisServiceResult luisServiceResult)
        {
            ICommand commandAuth = CommandFactory.FactoryMethod(_turn, _accessors, CommandAuthentication);
            await commandAuth.ExecuteCommandAsync();
            return await _scenarioDialogs.CreateContextAsync(_turn).Result.EndDialogAsync();
        }

        public bool NeedAuthentication()
        {
            return false;
        }
    }
}