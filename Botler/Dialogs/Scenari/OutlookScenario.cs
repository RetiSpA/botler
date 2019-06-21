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
            _scenarioDialogs.Add(new LetturaMailOutlook(_accessors, ScenarioIntent));
            _scenarioDialogs.Add(new PrenotaSalaRiunioni(ScenarioIntent));
            _scenarioDialogs.Add(new CreaAppuntamentoCalendar(ScenarioIntent, _accessors));
            _scenarioDialogs.Add(new VisualizzaAppuntamentiCalendar(_accessors, ScenarioIntent));
            DialogContext currentDialogContext = await GetDialogContextAsync();
            
            

            DialogTurnResult dialogResult = null;

            dialogResult = await currentDialogContext.ContinueDialogAsync();
            
            switch (dialogResult.Status)
                {
                    case DialogTurnStatus.Empty:
                    {
                        await currentDialogContext.BeginDialogAsync(ScenarioIntent.DialogID);
                        break;
                    }

                    case DialogTurnStatus.Waiting:
                        break;

                    case DialogTurnStatus.Complete:
                    {
                        await currentDialogContext.EndDialogAsync();
                        break;
                    }

                    default:
                    {
                        await currentDialogContext.CancelAllDialogsAsync();
                        break;
                    }
                }           
            // if (OutlookIntents.Contains(ScenarioIntent.Name))
            // {
            //     await currentDialogContext.BeginDialogAsync(ScenarioIntent.DialogID);
            // }
        }

    }
}