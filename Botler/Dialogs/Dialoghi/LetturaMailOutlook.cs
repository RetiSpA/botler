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
using Botler.Helpers;
using Botler.Models;
using System.Text.RegularExpressions;

namespace Botler.Dialogs.Dialoghi
{
    public class LetturaMailOutlook : ComponentDialog
    {
        // Dialogs IDs
        private const string LetturaMailDialog = "letturaMailDialog";

        private Intent _intent;

        public LetturaMailOutlook(Intent intent) : base(nameof(LetturaMailOutlook))
        {
            _intent = intent ?? throw new ArgumentNullException(nameof(intent));
            _intent.EntitiesCollected = EntityFormatHelper.FiltrEntityByIntent(_intent.Name, _intent.EntitiesCollected);

            var waterfallSteps = new WaterfallStep[]
            {
                ReadMailAsync,
            };

            AddDialog(new WaterfallDialog(LetturaMailDialog, waterfallSteps));
        }

        public DateTime Date { get; set; }

        public int numbersMail { get; set; } = 10;

        public bool MailUnread { get; set; } = false;

        public string DialogID { get; private set; } = nameof(LetturaMailDialog);

        private async Task<DialogTurnResult> ReadMailAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // ("lettura mail ");
            // // * LOGICA: in base alle entità che ho chiamo una funzione dalle GraphAPIHelper * //
            // // TODO: Ultima verifca alle entità e creare entità nel formato necessario
            // var token = await _accessors.GetUserToken(stepContext.Context);
            // await GraphAPIHelper.GetMailsFromInboxAsync(stepContext.Context, _accessors, token.Token, dateTime, numbersMail, mailUnread);
            var provaEntità = string.Empty;

      // ** Lettura entità ** //
            foreach(var e in _intent.EntitiesCollected)
            {
                if(!e.Type.Equals("$instance"))
                {
                    provaEntità = e.Text;
                    await stepContext.Context.SendActivityAsync("Lettura mail del giorno " + provaEntità).ConfigureAwait(true);
                }
            }

            return await stepContext.EndDialogAsync();
        }

    }
}