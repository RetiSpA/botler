
using System;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Resources;
using System.Globalization;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs;
using Botler.Controller;
using System.Threading.Tasks;
using Botler.Middleware.Services;
using Botler.Models;
using Microsoft.Bot.Builder;

namespace Botler.Dialogs.Scenari
{
    public interface IScenario
    {
        Intent ScenarioIntent{ get; set; }

        bool NeedAuthentication { get; set; }

        string ScenarioID { get; set; }

        string AssociatedScenario { get; set; }

        Task HandleScenarioStateAsync(ITurnContext turn, BotlerAccessors accessors, LuisServiceResult luisServiceResult);
    }
}