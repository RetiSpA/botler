using System;
using System.Threading.Tasks;
using Botler.Dialogs.Dialoghi;
using Botler.Middleware.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Botler.Builders;
using Botler.Controllers;
using Botler.Models;
using Botler.Helpers;
using static Botler.Dialogs.Utility.Scenari;
using static Botler.Dialogs.Utility.LuisIntent;
using static Botler.Dialogs.Utility.IntentsSets;

namespace Botler.Dialogs.Scenari
{
    public class OutlookScenario : ExecutionScenario
    {
        private readonly BotlerAccessors _accessors;

        private readonly ITurnContext _turn;

        private readonly DialogSet _scenarioDialogs;

        public override string ScenarioID { get; set; } = Outlook;

        public override bool NeedAuthentication { get ; set; } = true;

        public override Intent ScenarioIntent { get; set; }

        public override string AssociatedScenario { get; set; } = OutlookDescription;

        public OutlookScenario(BotlerAccessors accessors, ITurnContext turn)
        {
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            _turn = turn ?? throw new ArgumentNullException(nameof(accessors));

            _scenarioDialogs = new DialogSet(accessors.DialogStateAccessor);
        }

        public async Task<DialogContext> GetDialogContextAsync()
        {
            DialogContext currentDialogContext = await _scenarioDialogs.CreateContextAsync(_turn);
            return currentDialogContext;
        }

        public override async Task CreateResponseAsync(LuisServiceResult luisServiceResult)
        {
            DialogContext currentDialogContext = await GetDialogContextAsync();

            _scenarioDialogs.Add(new LetturaMailOutlook(ScenarioIntent));
            _scenarioDialogs.Add(new PrenotaSalaRiunioni(ScenarioIntent));


            if (OutlookIntents.Contains(ScenarioIntent.Name))
            {
                if (ScenarioIntent.EntitiesCollected.Count == 0)
                {
                    // * Legge i JSON dal CosmoDB per trovare entità utili per questo ScenarioIntent * //
                    // * Se non ne trova, manda messaggio all'utente di inserirle * //
                    await _turn.SendActivityAsync("Start dialog == 0 entities");

                }
                if (ScenarioIntent.EntitiesCollected.Count > 0)
                {
                    // var currentBotState = await BotStateBuilder.BuildAndSaveBotStateContextContext(_turn, _accessors, luisServiceResult, ScenarioID, ScenarioIntent);
                    // await currentDialogContext.BeginDialogAsync(ScenarioIntent.DialogID);
                    await _turn.SendActivityAsync("Starting dialog " + ScenarioIntent.DialogID);
                }
            }
        }
    }
}