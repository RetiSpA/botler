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
using Botler.Controller;

namespace Botler.Dialogs.Dialoghi
{
    public class AutenticaUtente : ComponentDialog
    {
        private readonly BotlerAccessors _accessors;

        private const string AuthdDialog = "authDialog";


        public AutenticaUtente(BotlerAccessors accessors) : base(nameof(AutenticaUtente))
        {
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));

            var WaterfallSteps = new WaterfallStep[]
            {
                AuthenticationStepAsync,
            };

            AddDialog(new WaterfallDialog(AuthdDialog, WaterfallSteps));
        }

        private async Task<DialogTurnResult> AuthenticationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var message = stepContext.Context.Activity.Text;

            if (AuthenticationHelper.MagicCodeFound(message))
            {
                var turn = stepContext.Context;
                await AuthenticationHelper.SecondPhaseAuthAsync(turn, _accessors);
            }
            else
            {
                var turn = stepContext.Context;
                await AuthenticationHelper. FirstPhaseAuthAsync(turn, _accessors);
            }

            return await stepContext.EndDialogAsync();
        }
    }
}