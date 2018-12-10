// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace Botler.Dialogs.Dialoghi
{
    /// <summary>
    /// Demonstrates the following concepts:
    /// - Use a subclass of ComponentDialog to implement a multi-turn conversation
    /// - Use a Waterflow dialog to model multi-turn conversation flow
    /// - Use custom prompts to validate user input
    /// - Store conversation and user state.
    /// </summary>
    public class Presentazione : ComponentDialog
    {
        // User state for greeting dialog
        private const string GreetingStateProperty = "greetingState";
        private const string NameValue = "greetingName";

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
        public Presentazione(IStatePropertyAccessor<PresentazioneModel> userProfileStateAccessor, ILoggerFactory loggerFactory)
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

        public IStatePropertyAccessor<PresentazioneModel> UserProfileAccessor { get; }

        private async Task<DialogTurnResult> InitializeStateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var greetingState = await UserProfileAccessor.GetAsync(stepContext.Context, () => null);
            if (greetingState == null)
            {
                var greetingStateOpt = stepContext.Options as PresentazioneModel;
                if (greetingStateOpt != null)
                {
                    await UserProfileAccessor.SetAsync(stepContext.Context, greetingStateOpt);
                }
                else
                {
                    await UserProfileAccessor.SetAsync(stepContext.Context, new PresentazioneModel());
                }
            }

            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> PromptForNameStepAsync(
                                                WaterfallStepContext stepContext,
                                                CancellationToken cancellationToken)
        {
            var greetingState = await UserProfileAccessor.GetAsync(stepContext.Context);

            // Se son già presenti i dati richiesti, saluta l'utente e ritorna.
            if (greetingState != null && !string.IsNullOrWhiteSpace(greetingState.Name))
            {
                return await GreetUser(stepContext);
            }

            // Se non è già inserito io nome, richiede l'inserimento del nome.
            if (string.IsNullOrWhiteSpace(greetingState.Name))
            {
                // richiesta del nome se mancante.
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

        // Funzione per salutare l'utente.
        private async Task<DialogTurnResult> DisplayGreetingStateStepAsync(
                                                    WaterfallStepContext stepContext,
                                                    CancellationToken cancellationToken)
        {
            // Salva il nome se inserito.
            var greetingState = await UserProfileAccessor.GetAsync(stepContext.Context);

            var lowerCaseName = stepContext.Result as string;
            if (string.IsNullOrWhiteSpace(greetingState.Name) &&
                !string.IsNullOrWhiteSpace(lowerCaseName))
            {
                // Mette l'iniziale del nome maiuscola e setta il nome.
                greetingState.Name = char.ToUpper(lowerCaseName[0]) + lowerCaseName.Substring(1);
                await UserProfileAccessor.SetAsync(stepContext.Context, greetingState);
            }

            return await GreetUser(stepContext);
        }

        /// <summary>
        /// Funzione per verificare la validità del nome inserito.
        /// </summary>
        /// <param name="promptContext">Context for this prompt.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> that represents the work queued to execute.</returns>
        private async Task<bool> ValidateName(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            // Validate that the user entered a minimum length for their name.
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

        // Funzione per salutare l'utente con le info presenti in GreetingState.
        private async Task<DialogTurnResult> GreetUser(WaterfallStepContext stepContext)
        {
            var context = stepContext.Context;
            var greetingState = await UserProfileAccessor.GetAsync(context);

            // Display their profile information and end dialog.
            await context.SendActivityAsync($"Piacere {greetingState.Name}, mi chiamo Botler! Come posso esserti d'aiuto?");
            return await stepContext.EndDialogAsync();
        }
    }
}
