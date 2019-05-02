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
    public class MenuDipedentiScenario : IScenario
    {
        private readonly DialogSet _dialogs;

        public MenuDipedentiScenario(BotlerAccessors accessors)
        {
            _dialogs = new DialogSet(accessors.DialogStateAccessor);
            _dialogs.Add(new MenuDipendenti(accessors));
        }

        public Dialog GetDialogByID(string ID)
        {
            throw new NotImplementedException();
        }

        public DialogSet GetDialogSet()
        {
            return _dialogs;
        }

        public string GetScenarioID()
        {
            return "MenuDipendenti";
        }

        public bool isAnAuthScenario()
        {
            return false;
        }
    }
}