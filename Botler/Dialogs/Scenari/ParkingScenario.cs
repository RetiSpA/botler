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
    public class ParkingScenario : IScenario
    {
        private readonly BotlerAccessors _accessors;

        public ParkingScenario(BotlerAccessors accessors)
        {
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            ILoggerFactory loggerFactory = new LoggerFactory();
            ScenarioDialogs = new DialogSet(accessors.DialogStateAccessor);
            
            ScenarioDialogs.Add(new Prenotazione(_accessors.PrenotazioneStateAccessor, loggerFactory));
            ScenarioDialogs.Add(new CancellaPrenotazione(_accessors.CancellaPrenotazioneStateAccessor, loggerFactory));
            ScenarioDialogs.Add(new VisualizzaTempo(_accessors.VisualizzaTempoStateAccessor, loggerFactory));
            ScenarioDialogs.Add(new VisualizzaPrenotazione(_accessors.VisualizzaPrenotazioneStateAccessor, loggerFactory));
        }

        private DialogSet ScenarioDialogs;

        public TokenResponse tokenAuth { get; set; }

        public Dialog GetDialogByID(string idDialog)
        {
            return ScenarioDialogs.Find(nameof(idDialog));
        }

        public string GetScenarioID()
        {
            return "Parking";
        }

        public DialogSet GetDialogSet()
        {
            return ScenarioDialogs;
        }

        public bool isAnAuthScenario()
        {
            return true;
        }

    }
}