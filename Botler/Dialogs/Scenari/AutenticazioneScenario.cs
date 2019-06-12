using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Botler.Dialogs.RisorseApi;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Botler.Controller;
using Microsoft.Extensions.Logging;
using Botler.Dialogs.Utility;
using Botler.Commands;
using Botler.Models;
using Botler.Middleware.Services;

using static Botler.Dialogs.Utility.Responses;
using static Botler.Dialogs.Utility.Commands;
using static Botler.Dialogs.Utility.ListsResponsesIT;
using static Botler.Dialogs.Utility.Scenari;
using Botler.Dialogs.Dialoghi;
using Botler.Builders;

namespace Botler.Dialogs.Scenari
{
    public class AutenticazioneScenario : ExecutionScenario
    {
        private readonly DialogSet _scenarioDialogs;

        private readonly ITurnContext _turn;

        private readonly BotlerAccessors _accessors;

        public override string ScenarioID { get; set; } = Autenticazione;

        public override Intent ScenarioIntent { get; set; }

        public override bool NeedAuthentication { get; set; } = false;

        public override string AssociatedScenario { get; set; } = Autenticazione;

        public AutenticazioneScenario(BotlerAccessors accessors, ITurnContext turn )
        {
            _scenarioDialogs = new DialogSet(accessors.DialogStateAccessor);
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            _turn = turn ?? throw new ArgumentNullException(nameof(turn));
            _scenarioDialogs.Add(new AutenticaUtente(accessors));
        }

        public async Task<DialogContext> GetDialogContextAsync()
        {
            DialogContext currentDialogContext = await _scenarioDialogs.CreateContextAsync(_turn);
            return currentDialogContext;
        }

        public override async Task CreateResponseAsync(LuisServiceResult luisServiceResult)
        {
            // ICommand commandAuth = CommandFactory.FactoryMethod(_turn, _accessors, CommandAuthentication);
            // await commandAuth.ExecuteCommandAsync();
            var dialogContext = await _scenarioDialogs.CreateContextAsync(_turn);
            await dialogContext.BeginDialogAsync(nameof(AutenticaUtente));
        }
    }
}