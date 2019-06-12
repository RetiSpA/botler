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

namespace Botler.Dialogs.Dialoghi
{
    public class PrenotaSalaRiunioni : ComponentDialog
    {
        private const string PrenotaSalaRiunioniDialog = "prenotaSalaRiunioniDialog";

        private Intent _intent;

        public PrenotaSalaRiunioni(Intent intent) : base(nameof(PrenotaSalaRiunioni))
        {
            _intent = intent ?? throw new ArgumentNullException(nameof(intent));
            _intent.EntitiesCollected = EntityFormatHelper.FiltrEntityByIntent(_intent.Name, _intent.EntitiesCollected);

            var waterfallSteps = new WaterfallStep[]
            {
                PrenotaSalaAsync,
            };

            AddDialog(new WaterfallDialog(PrenotaSalaRiunioniDialog, waterfallSteps));
        }

        public string Sala { get; set; }

        public DateTime Date { get; set; }
        

        private async Task<DialogTurnResult> PrenotaSalaAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            foreach(var e in _intent.EntitiesCollected)
            {
                var provaEntità = string.Empty;
                if(!e.Type.Equals("$instance"))
                {
                    provaEntità = e.Text;
                    await stepContext.Context.SendActivityAsync("Prenotazione sala riunioni giorno" + provaEntità).ConfigureAwait(true);
                }
            }

            return await stepContext.EndDialogAsync();
        }

    }
}