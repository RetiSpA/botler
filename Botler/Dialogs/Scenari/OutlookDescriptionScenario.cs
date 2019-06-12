using System;
using System.Threading.Tasks;
using Botler.Builders;
using Botler.Models;
using Botler.Middleware.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using static Botler.Dialogs.Utility.Scenari;

namespace Botler.Dialogs.Scenari
{
    public class OutlookDescriptionScenario : DescriptionScenario
    {
        private readonly BotlerAccessors _accessors;
        private readonly ITurnContext _turn;

        public OutlookDescriptionScenario(BotlerAccessors accessors, ITurnContext turn)
        {
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            _turn = turn ?? throw new ArgumentNullException(nameof(turn));
        }

        public override Intent ScenarioIntent { get;  set; }

        public override string ScenarioID { get; set; } = OutlookDescription;

        public override string AssociatedScenario { get; set; } = Outlook;

        public override bool NeedAuthentication { get; set; } = true;
    }
}