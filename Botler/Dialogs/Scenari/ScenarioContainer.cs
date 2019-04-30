using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;
using Botler.Dialogs.RisorseApi;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Botler.Dialogs.Utility;
using Botler.Controller;
using Microsoft.Extensions.Logging;

namespace Botler.Dialogs.Scenari
{
    public class ScenarioContainer
    {
        private readonly IStatePropertyAccessor<DialogState> _dialogState;

        private readonly BotlerAccessors _accessors;

        public ScenarioContainer(BotlerAccessors accessors)
        {
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));

            ListScenari  = new List<IScenario>();

            LoadScenari();
        }

        public Dictionary<string, DialogSet> DialogsMap { get; set; }

        public IList<IScenario> ListScenari { get; set; }

        private void LoadScenari()
        {
            ListScenari.Add(new ParkingScenario(_accessors));

        }
    }
}