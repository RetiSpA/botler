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
    public class SupportoScenario : ExecutionScenario
    {
        private readonly ITurnContext _turn;
        private readonly BotlerAccessors _accessors;
        private readonly DialogSet _scenarioDialogs;

        public SupportoScenario(BotlerAccessors accessors, ITurnContext turn)
        {
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            _turn = turn ?? throw new ArgumentNullException(nameof(accessors));

            _scenarioDialogs = new DialogSet(accessors.DialogStateAccessor);
        }

        public override Intent ScenarioIntent { get; set; }

        public override bool NeedAuthentication { get; set; } = true;

        public override string ScenarioID { get; set; } = Supporto;

        public override string AssociatedScenario { get; set; } = SupportoDescription;

        public override async Task CreateResponseAsync(LuisServiceResult luisServiceResult)
        {
             _scenarioDialogs.Add(new CreaTicket(ScenarioIntent, _accessors));
            DialogContext currentDialogContext = await _scenarioDialogs.CreateContextAsync(_turn);
            var dialogResult = await currentDialogContext.ContinueDialogAsync();

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
            // await currentDialogContext.BeginDialogAsync(ScenarioIntent.DialogID);

        }
    }
}