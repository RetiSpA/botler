// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Botler.Dialogs.RisorseApi;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace Botler.Dialogs.Dialoghi
{
    public class Presentazione : ComponentDialog
    {
        // Prompts names
        private const string NamePrompt = "namePrompt";

        // Minimum length requirements for name
        private const int NameLengthMinValue = 2;

        // Dialog IDs
        private const string ProfileDialog = "profileDialog";

        /// <summary>
        /// Initializes a new instance of the <see cref="Presentazione"/> class.
        /// </summary>
        /// <param name="botServices">Connected services used in processing.</param>
        /// <param name="botState">The <see cref="UserState"/> for storing properties at user-scope.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> that enables logging and tracing.</param>
        public Presentazione(IStatePropertyAccessor<UserModel> userProfileStateAccessor, ILoggerFactory loggerFactory)
            : base(nameof(Presentazione))
        {
            UserProfileAccessor = userProfileStateAccessor ?? throw new ArgumentNullException(nameof(userProfileStateAccessor));

            // Add control flow dialogs
            var waterfallSteps = new WaterfallStep[]
            {
                    InitializeStateStepAsync,
                    PromptForNameStepAsync,
                    DisplayGreetingStateStepAsync,
            };
            AddDialog(new WaterfallDialog(ProfileDialog, waterfallSteps));
            AddDialog(new TextPrompt(NamePrompt, ValidateName));

        }

        public IStatePropertyAccessor<UserModel> UserProfileAccessor { get; }

        private async Task<DialogTurnResult> InitializeStateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            var greetingState = await UserProfileAccessor.GetAsync(stepContext.Context, () => null);
            if (greetingState == null)
            {
                var greetingStateOpt = stepContext.Options as UserModel;
                if (greetingStateOpt != null)
                {
                    await UserProfileAccessor.SetAsync(stepContext.Context, greetingStateOpt);
                }
                else
                {
                    await UserProfileAccessor.SetAsync(stepContext.Context, new UserModel(0,null));
                }
            }

            return await stepContext.NextAsync();
        }

        // Funzione che richiede il nome.
        private async Task<DialogTurnResult> PromptForNameStepAsync(
                                                WaterfallStepContext stepContext,
                                                CancellationToken cancellationToken)
        {
            var greetingState = await UserProfileAccessor.GetAsync(stepContext.Context);

            // Se son già presenti i dati richiesti, saluta l'utente e ritorna.
            if (greetingState != null && !string.IsNullOrWhiteSpace(greetingState.nome))
            {
                return await GreetUser(stepContext);
            }

            // Se non è già inserito il nome, richiede l'inserimento.
            if (string.IsNullOrWhiteSpace(greetingState.nome))
            {
                var opts = new PromptOptions
                {
                    Prompt = new Activity
                    {
                        Type = ActivityTypes.Message,
                        Text = "Ciao! Qual'è il tuo nome?",
                    },
                };
                return await stepContext.PromptAsync(NamePrompt, opts);
            }
            else
            {
                return await stepContext.NextAsync();
            }
        }

        // Funzione per salvare il nome l'utente.
        private async Task<DialogTurnResult> DisplayGreetingStateStepAsync(
                                                    WaterfallStepContext stepContext,
                                                    CancellationToken cancellationToken)
        {
            // Salva il nome.
            var greetingState = await UserProfileAccessor.GetAsync(stepContext.Context);

            var lowerCaseName = stepContext.Result as string;
            if (string.IsNullOrWhiteSpace(greetingState.nome) &&
                !string.IsNullOrWhiteSpace(lowerCaseName))
            {
                // Mette l'iniziale del nome maiuscola e lo setta.
                greetingState.nome = char.ToUpper(lowerCaseName[0]) + lowerCaseName.Substring(1);
                await UserProfileAccessor.SetAsync(stepContext.Context, greetingState);
            }

            return await GreetUser(stepContext);
        }

        // Funzione per verificare la lunghezza del nome.
        private async Task<bool> ValidateName(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var value = promptContext.Recognized.Value?.Trim() ?? string.Empty;
            if (value.Length > NameLengthMinValue)
            {
                promptContext.Recognized.Value = value;
                return true;
            }
            else
            {
                await promptContext.Context.SendActivityAsync($"Il nome deve essere lungo almeno `{NameLengthMinValue}` caratteri.").ConfigureAwait(false);
                return false;
            }
        }

        // Funzione per salutare l'utente.
        private async Task<DialogTurnResult> GreetUser(WaterfallStepContext stepContext)
        {
            var context = stepContext.Context;
            var greetingState = await UserProfileAccessor.GetAsync(context);

            // Display their profile information and end dialog.
            await context.SendActivityAsync($"Piacere {greetingState.nome}, mi chiamo Botler! Come posso esserti d'aiuto?");
            return await stepContext.EndDialogAsync();
        }
    }
}
