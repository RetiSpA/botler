using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Botler.Dialogs.Dialoghi;
using Botler.Dialogs.RisorseApi;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Botler.Dialogs.Utility;
using Microsoft.Extensions.Logging;
using Microsoft.Bot.Schema;
using Botler.Controller;
using Botler.Middleware.Services;
using Botler.Controllers;
using Botler.Models;
using Botler.Builders;

using static Botler.Dialogs.Utility.BotConst;
using static Botler.Dialogs.Utility.LuisIntent;
using static Botler.Dialogs.Utility.ListsResponsesIT;
using static Botler.Dialogs.Utility.Responses;
using static Botler.Dialogs.Utility.Scenari;


namespace Botler.Dialogs.Scenari
{
    public class DefaultScenario : IScenario
    {
        private readonly BotlerAccessors _accessors;

        private readonly ITurnContext _turn;

        private readonly DialogSet _scenarioDialogs;

        public string ScenarioID { get; set; } = Default;

        public Intent ScenarioIntent { get; set; }

        public string AssociatedScenario { get; set; } = "Default";

        public DefaultScenario(BotlerAccessors accessors, ITurnContext turn)
        {
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            _turn = turn ?? throw new ArgumentNullException(nameof(turn));

            _scenarioDialogs = new DialogSet(_accessors.DialogStateAccessor);
        }

        public Dialog GetDialogByID(string idDialog)
        {
            return _scenarioDialogs.Find(nameof(idDialog));
        }

        public async Task CreateResponseAsync(LuisServiceResult luisServiceResult)
        {
           await  _turn.SendActivityAsync(RandomResponses(NoneResponse) + "\n" + RandomResponses(PossibilitaResponse));
        }

        public bool NeedAuthentication { get; set; } = false;

        public async  Task<DialogContext> GetDialogContextAsync()
        {
           var dialogContext = await _scenarioDialogs.CreateContextAsync(_turn);
           return dialogContext;
        }

        public async Task HandleScenarioStateAsync(ITurnContext turn, BotlerAccessors accessors, LuisServiceResult luisServiceResult)
        {
            await CreateResponseAsync(luisServiceResult);
        }
    }
}