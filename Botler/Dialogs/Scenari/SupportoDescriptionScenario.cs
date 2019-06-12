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
    public class SupportoDescriptionScenario : DescriptionScenario
    {
        public override bool NeedAuthentication { get; set; } = true;

        public override string ScenarioID { get; set; } = SupportoDescription;

        public override Intent ScenarioIntent { get; set; }

        public override string AssociatedScenario { get; set; } = Supporto;
    }
}