using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Botler.Dialogs.RisorseApi;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Botler.Dialogs.Utility;
using Microsoft.Extensions.Logging;
using static Botler.Dialogs.Utility.Responses;
using static Botler.Dialogs.Utility.ListsResponsesIT;
using Botler.Dialogs.Scenari;

namespace Botler.Dialogs.Dialoghi
{
    public class MenuDipendenti : ComponentDialog
    {
        private BotlerAccessors _accessors;

        private const string MenuPrompt = "MenuPrompt";

        private const string MenuDialog = "MenuDialog";

        private const string ParkingPrompt = "ParkingPrompt";

        private string sceltaScenario = string.Empty;

        public MenuDipendenti(BotlerAccessors accessors) : base(nameof(MenuDipendenti))
        {
            _accessors = accessors ?? throw new ArgumentNullException(nameof(MenuDipendenti));

            var waterfalls = new WaterfallStep[] {
                MenuPromptStepAsync,
                RispostaPromptStepAsync,
                VisualizzaSceltaPromptStepAsync,
            };

            AddDialog(new WaterfallDialog(MenuDialog, waterfalls));
            AddDialog(new ChoicePrompt(ParkingPrompt));
            AddDialog(new ChoicePrompt(MenuPrompt));
        }

        private async Task<DialogTurnResult> VisualizzaSceltaPromptStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (sceltaScenario.Equals("Parking"))
            {
                var context = stepContext.Context;

                return await stepContext.PromptAsync("ParkingPrompt", new PromptOptions
                {
                    Prompt = MessageFactory.Text("Ok, ecco cosa puoi fare:"),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "Prenotare un parcheggio", "Visualizzare Prenotazione", "Visualizzare sTempo rimanente Prenotazione"
                    , "Eliminare Prenotazione" })
                }, cancellationToken);
            }
            return null;
        }

        private async Task<DialogTurnResult> RispostaPromptStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var scenario = (stepContext.Result as FoundChoice).Value;
            var context = stepContext.Context;

            if (scenario.Equals("Parking"))
            {
               // ParkingScenario Parking = new ParkingScenario(_accessors);
                await _accessors.ScenarioStateAccessors.SetAsync(context, "Parking");
                sceltaScenario = "Parking";
                await stepContext.NextAsync();
            }

            else
            {
               // DefaultScenario Default = new DefaultScenario(_accessors);
                await _accessors.ScenarioStateAccessors.SetAsync(context, "DefaultScenario");
            }

            return await stepContext.EndDialogAsync();
        }

        private async Task<DialogTurnResult> MenuPromptStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var context = stepContext.Context;

            return await stepContext.PromptAsync("MenuPrompt", new PromptOptions
            {
                Prompt = MessageFactory.Text("Ecco cosa posso fare per te:"),
                RetryPrompt = MessageFactory.Text("Scegli un opzione dall'elenco!"),
                Choices = ChoiceFactory.ToChoices(new List<string> { "Parking"}),
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> InitialMenuStateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
           return await stepContext.NextAsync();
        }
    }
}