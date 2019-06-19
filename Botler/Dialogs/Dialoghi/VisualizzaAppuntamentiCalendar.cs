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
using Botler.Helpers;
using Botler.Models;
using System.Text.RegularExpressions;
using static Botler.Dialogs.Utility.Responses;
using static Botler.Dialogs.Utility.ListsResponsesIT;
using static Botler.Dialogs.Utility.LuisEntity;


namespace Botler.Dialogs.Dialoghi
{
    public class VisualizzaAppuntamentiCalendar : ComponentDialog
    {
        private Intent _intent;

        private BotlerAccessors _accessors;

        private const string VisualizzaAppuntamentiCalendarDialog = "visualizzaAppuntamentiCalendarDilaog";

        public VisualizzaAppuntamentiCalendar(BotlerAccessors accessors, Intent intent) : base(nameof(VisualizzaAppuntamentiCalendar))
        {
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            _intent = intent ?? throw new ArgumentNullException(nameof(intent));
            _intent.EntitiesCollected = EntityFormatHelper.EntityFilterByIntent(_intent.Name, _intent.EntitiesCollected);

            var waterfallSteps = new WaterfallStep[]
            {
                VisualizzaAppuntamentiDialogAsync,
            };

            AddDialog(new WaterfallDialog(VisualizzaAppuntamentiCalendarDialog, waterfallSteps));
        }

        public DateTime Date { get; set; }

        private async Task<DialogTurnResult> VisualizzaAppuntamentiDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            ParseEntity();

            var tokenResponse = await _accessors.GetUserToken(stepContext.Context);

            if (Date == DateTime.MinValue)
            {
                Date = DateTime.Now;
            }

            await GraphAPIHelper.GetAppointmentOnCalendarAsync(stepContext.Context, _accessors, tokenResponse.Token, Date);

            return await stepContext.EndDialogAsync();
        }

        private void ParseEntity()
        {
            foreach(var e in _intent.EntitiesCollected)
            {
                if (e.Type.Equals(Datetime))
                {
                    Date = DateTime.Parse(e.Text);
                }
            }
        }

    }
}