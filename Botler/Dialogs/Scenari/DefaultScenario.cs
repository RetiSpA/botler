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

namespace Botler.Dialogs.Scenari
{
    public class DefaultScenario : IScenario
    {
        public DefaultScenario(BotlerAccessors accessors)
        {
            ScenarioDialogs = new DialogSet(accessors.DialogStateAccessor);
        }

        private DialogSet ScenarioDialogs;

        public Dialog GetDialogByID(string idDialog)
        {
            return ScenarioDialogs.Find(nameof(idDialog));
        }

        public string GetScenarioID()
        {
            return "Default";
        }

        public DialogSet GetDialogSet()
        {
            return ScenarioDialogs;
        }

        public bool isAnAuthScenario()
        {
            return false;
        }
    }
}