
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
using Botler.Services;

namespace Botler.Dialogs.Scenari
{
    public interface IScenario
    {
        Task<DialogContext> GetDialogContextAsync();

        Task<DialogTurnResult> HandleDialogResultStatusAsync(LuisServiceResult luisServiceResult);

        bool NeedAuthentication();
    }
}