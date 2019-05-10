// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Resources;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Net.Http;
using Microsoft.Bot.Builder.AI.QnA;
using System.Threading.Tasks;
using Botler.Dialogs.Dialoghi;
using Botler.Dialogs.RisorseApi;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Botler.Dialogs.Utility;
using Botler;
using Newtonsoft.Json.Linq;
using Botler.Controller;
using static Botler.Dialogs.Utility.BotConst;

namespace Botler
{
    /// <summary>
    /// Main entry point and orchestration for bot.
    /// </summary>
    public class Botler : IBot
    {

        private bool autenticazione = false;
        public static bool askCredential = false;
        public static DateTime tempoPrenotazione;
        public static Boolean prenotazione = false;
        public static string email;
        public static string password;
        public static Boolean procedure = false;
        public static int BotId = 8;

        /// <summary>
        /// Key in the bot config (.bot file) for the LUIS instance.
        /// In the .bot file, multiple instances of LUIS can be configured.
        /// </summary>
        private readonly BotlerAccessors _accessors;
        private readonly BotServices _services;

        private TurnController TurnController;

        /// <summary>
        /// Initializes a new instance of the <see cref="Botler"/> class.
        /// </summary>
        /// <param name="botServices">Bot services.</param>
        /// <param name="accessors">Bot State Accessors.</param>
        public Botler(BotServices services, BotlerAccessors accessors, ILoggerFactory loggerFactory)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));

            // Verifica la configurazione di LUIS.
            if (!_services.LuisServices.ContainsKey(LuisConfiguration))
            {
                throw new InvalidOperationException($"The bot configuration does not contain a service type of `luis` with the id `{LuisConfiguration}`.");
            }

            //Verifica la configurazione di QnA.
            if (!_services.QnAServices.ContainsKey(QnAPublicKey))
            {
                throw new InvalidOperationException($"The bot configuration does not contain a service type of `QnA` with the name `{QnAPublicKey}`.");
            }

            TurnController = new TurnController(_accessors, _services);
        }

        /// <summary>
        /// Run every turn of the conversation.
        /// Gives the responsability to handle the orchestration of messages to TurnController
        /// </summary>
        /// <param name="turnContext">Bot Turn Context.</param>
        /// <param name="cancellationToken">Task CancellationToken.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
             await TurnController.TurnHandlerAsync(turnContext, cancellationToken);
        }
    }
}
