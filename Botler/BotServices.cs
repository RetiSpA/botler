// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Configuration;

namespace Botler
{
    /// <summary>
    /// Represents references to external services.
    ///
    /// For example, LUIS services are kept here as a singleton.  This external service is configured
    /// using the <see cref="BotConfiguration"/> class.
    /// </summary>
    /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1"/>
    /// <seealso cref="https://www.luis.ai/home"/>
    public class BotServices
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BotServices"/> class.
        /// </summary>
        /// <param name="luisServices">A dictionary of named <see cref="LuisRecognizer"/> instances for usage within the bot.</param>
        public BotServices(BotConfiguration botConfiguration)
        {
            foreach (var service in botConfiguration.Services)
            {
                switch (service.Type)
                {
                    case ServiceTypes.QnA:
                        {
                            var qna = (QnAMakerService)service;
                            if (qna == null)
                            {
                                throw new InvalidOperationException("The QnA service is not configured correctly in your '.bot' file.");
                            }

                            if (string.IsNullOrWhiteSpace(qna.KbId))
                            {
                                throw new InvalidOperationException("The QnA KnowledgeBaseId ('kbId') is required to run this sample. Please update your '.bot' file.");
                            }

                            if (string.IsNullOrWhiteSpace(qna.EndpointKey))
                            {
                                throw new InvalidOperationException("The QnA EndpointKey ('endpointKey') is required to run this sample. Please update your '.bot' file.");
                            }

                            if (string.IsNullOrWhiteSpace(qna.Hostname))
                            {
                                throw new InvalidOperationException("The QnA Host ('hostname') is required to run this sample. Please update your '.bot' file.");
                            }

                            var qnaEndpoint = new QnAMakerEndpoint()
                            {
                                KnowledgeBaseId = qna.KbId,
                                EndpointKey = qna.EndpointKey,
                                Host = qna.Hostname,
                            };

                            var qnaMaker = new QnAMaker(qnaEndpoint);
                            this.QnAServices.Add(qna.Name, qnaMaker);
                            break;
                        }

                    case ServiceTypes.Luis:
                        {
                            var luis = (LuisService)service;
                            if (luis == null)
                            {
                                throw new InvalidOperationException("The LUIS service is not configured correctly in your '.bot' file.");
                            }

                            var app = new LuisApplication(luis.AppId, luis.AuthoringKey, luis.GetEndpoint());
                            var recognizer = new LuisRecognizer(app);
                            this.LuisServices.Add(luis.Name, recognizer);
                            break;
                        }

                    case ServiceTypes.Generic:
                    {
                        // Usiamo il tipo Generic nel file .bot per conservare le informaizoni necessarie
                        // per usare un servizio di tipo QnAMaker
                        var generic = (GenericService)service;
                        var qnaRiservata = new QnAMakerService();
                        string endpointKey = string.Empty;

                        generic.Configuration.TryGetValue("key", out endpointKey);   //"6dc26f54-145f-43ae-9165-afa2acbccf97";
                        qnaRiservata.Id = generic.Id;
                        qnaRiservata.KbId = generic.Id;
                        qnaRiservata.EndpointKey = endpointKey;                      //"31ab70e5-d078-454b-a2a8-b8cca517bf4d";
                        qnaRiservata.Hostname =  generic.Url;                        //"https://botler-qna.azurewebsites.net";
                        qnaRiservata.Name = generic.Name;                            // "botler-qna";

                        var qnaEndpoint = new QnAMakerEndpoint()
                            {
                                KnowledgeBaseId = qnaRiservata.KbId,
                                EndpointKey = qnaRiservata.EndpointKey,
                                Host = qnaRiservata.Hostname,
                            };

                        var qnaMaker = new QnAMaker(qnaEndpoint);
                        this.QnAServices.Add(qnaRiservata.Name, qnaMaker);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the set of LUIS Services used.
        /// Given there can be multiple <see cref="LuisRecognizer"/> services used in a single bot,
        /// LuisServices is represented as a dictionary.  This is also modeled in the
        /// ".bot" file since the elements are named.
        /// </summary>
        /// <remarks>The LUIS services collection should not be modified while the bot is running.</remarks>
        /// <value>
        /// A <see cref="LuisRecognizer"/> client instance created based on configuration in the .bot file.
        /// </value>
        public Dictionary<string, LuisRecognizer> LuisServices { get; } = new Dictionary<string, LuisRecognizer>();

        public Dictionary<string, QnAMaker> QnAServices { get; } = new Dictionary<string, QnAMaker>();
    }
}
