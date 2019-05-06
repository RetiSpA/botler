using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Botler.Dialogs.RisorseApi;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Botler.Dialogs.Utility;
using Microsoft.Extensions.Logging;
using static Botler.Dialogs.Utility.Responses;
using static Botler.Dialogs.Utility.ListsResponsesIT;

namespace Botler.Dialogs.Scenari
{
    public class AutenticazioneScenario : IScenario
    {
        private DialogSet _dialogs;

        public AutenticazioneScenario(BotlerAccessors accessors)
        {
            _dialogs = new DialogSet(accessors.DialogStateAccessor);
        }

        public DialogSet GetDialogSet()
        {
            return _dialogs;
        }

    }
}