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
using static Botler.Dialogs.Utility.BotConst;
using static Botler.Dialogs.Utility.LuisIntent;
using static Botler.Dialogs.Utility.ListsResponsesIT;
using static Botler.Dialogs.Utility.Responses;
using Botler.Services;

namespace Botler.Dialogs.Scenari
{
    public class DefaultScenario : IScenario
    {
        private readonly BotlerAccessors _accessors;

        private readonly ITurnContext _turn;

        public DefaultScenario(BotlerAccessors accessors, ITurnContext turn)
        {
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            _turn = turn ?? throw new ArgumentNullException(nameof(turn));

            ScenarioDialogs = new DialogSet(_accessors.DialogStateAccessor);
        }

        private DialogSet ScenarioDialogs;

        public Dialog GetDialogByID(string idDialog)
        {
            return ScenarioDialogs.Find(nameof(idDialog));
        }


        public DialogSet GetDialogSet()
        {
            return ScenarioDialogs;
        }

        public async Task<DialogTurnResult> HandleDialogResultStatusAsync(LuisServiceResult luisServiceResult)
        {
            // None intent handler
            var topIntent = luisServiceResult.TopScoringIntent.Item1; // intent
            var score = luisServiceResult.TopScoringIntent.Item2; // score

            if(topIntent.Equals(NoneIntent) || score < 0.75)
            {
                await _turn.SendActivityAsync(RandomResponses(NoneResponse));
            }
            return await ScenarioDialogs.CreateContextAsync(_turn).Result.EndDialogAsync();
        }

        public bool NeedAuthentication()
        {
            return false;
        }
    }
}