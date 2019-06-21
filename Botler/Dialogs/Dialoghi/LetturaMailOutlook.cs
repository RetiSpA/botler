using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Botler.Helpers;
using Botler.Models;
using System.Text.RegularExpressions;
using static Botler.Dialogs.Utility.Responses;
using static Botler.Dialogs.Utility.ListsResponsesIT;
using static Botler.Dialogs.Utility.LuisEntity;

namespace Botler.Dialogs.Dialoghi
{
    public class LetturaMailOutlook : ComponentDialog
    {
        // Dialogs IDs
        private const string LetturaMailDialog = "letturaMailDialog";

        private readonly Intent _intent;

        private readonly BotlerAccessors _accessors;

        public LetturaMailOutlook(BotlerAccessors accessors, Intent intent) : base(nameof(LetturaMailOutlook))
        {
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            _intent = intent ?? throw new ArgumentNullException(nameof(intent));
            _intent.EntitiesCollected = EntityFormatHelper.EntityFilterByIntent(_intent.Name, _intent.EntitiesCollected);
            var waterfallSteps = new WaterfallStep[]
            {
                ReadMailAsync,
            };

            AddDialog(new WaterfallDialog(LetturaMailDialog, waterfallSteps));
        }

        public DateTime Date { get; set; }

        public bool Unread { get; set; } = false;

        public string DialogID { get; private set; } = nameof(LetturaMailDialog);

        private async Task<DialogTurnResult> ReadMailAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            ParseEntity();

            var token = await _accessors.GetUserToken(stepContext.Context);
            await GraphAPIHelper.SendMailsFromInboxAsync(stepContext.Context,  token.Token, Date, Unread);
            //await stepContext.Context.SendActivityAsync("Leggo mail del " + Date.ToShortDateString());
            return await stepContext.EndDialogAsync();
        }

        private void ParseEntity()
        {
              // ** Lettura entità ** //
            foreach (var e in _intent.EntitiesCollected)
            {
                if (e.Type.Equals(Datetime))
                {
                    Console.WriteLine(e.Text);
                    Date = DateTime.Parse(e.Text);

                    if (Date.CompareTo(DateTime.Today.AddDays(1)) >= 0)
                    {
                        Date = DateTime.Now;
                    }
                }

                if (e.Type.Equals(MailUnread))
                {
                    Unread = true;
                }
            }
        }

    }
}