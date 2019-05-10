// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Botler.Dialogs.Scenari;
using Botler.Dialogs.Dialoghi;
using Botler.Dialogs.RisorseApi;
using Botler.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Botler.Controller;
using Botler.Dialogs.Utility;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Attachment = Microsoft.Bot.Schema.Attachment;
using static Botler.Dialogs.Utility.BotConst;
using static Botler.Dialogs.Utility.LuisIntent;
using static Botler.Dialogs.Utility.ListsResponsesIT;
using static Botler.Dialogs.Utility.Responses;
using static Botler.Dialogs.Utility.Scenari;


namespace Botler.Dialogs.Scenari
{
    public class ScenarioController
    {
        private readonly BotlerAccessors _accessors;

        private readonly Autenticatore _autenticatore;

        private DialogContext currentDialogContext;

        private IScenario currentScenario;

        private ITurnContext currentTurn;

        private LuisServiceResult luisServiceResult;

        private CancellationToken cancellationToken;


        public ScenarioController(BotlerAccessors accessors, ITurnContext currentTurn, LuisServiceResult luisServiceResult, IScenario currentScenario)
        {
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));

            this.currentScenario = currentScenario ?? throw new ArgumentNullException(nameof(currentScenario));

            _autenticatore = new Autenticatore();

            this.currentTurn = currentTurn ?? throw new ArgumentNullException(nameof(currentTurn));

            this.luisServiceResult = luisServiceResult ?? throw new ArgumentNullException(nameof(luisServiceResult));

        }

        public async Task<DialogTurnResult> HandleScenarioDialogAsync()
        {
            return await currentScenario.HandleDialogResultStatusAsync(luisServiceResult);
        }

    }
}