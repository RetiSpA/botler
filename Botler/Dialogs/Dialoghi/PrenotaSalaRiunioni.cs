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
using static Botler.Dialogs.Utility.EntitySets;
using static Botler.Dialogs.Utility.LuisEntity;

namespace Botler.Dialogs.Dialoghi
{
    public class PrenotaSalaRiunioni : ComponentDialog
    {
        private const string PrenotaSalaRiunioniDialog = "prenotaSalaRiunioniDialog";

        private Intent _intent;

        public PrenotaSalaRiunioni(Intent intent) : base(nameof(PrenotaSalaRiunioni))
        {
            _intent = intent ?? throw new ArgumentNullException(nameof(intent));
            _intent.EntitiesCollected = EntityFormatHelper.EntityFilterByIntent(_intent.Name, _intent.EntitiesCollected);

            var waterfallSteps = new WaterfallStep[]
            {
                PrenotaSalaAsync,
                ConfermaSalaAsync,
            };

            AddDialog(new WaterfallDialog(PrenotaSalaRiunioniDialog, waterfallSteps));
        }

        public string Sala { get; set; }

        public DateTime Date { get; set; }


        private async Task<DialogTurnResult> PrenotaSalaAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            ParseEntity();

            if (string.IsNullOrEmpty(Sala)) // * Potrebbe essere opzionale -> Prenota la prima sala libera */ 
            {
                // Asks for a specific meeting room
                await stepContext.Context.SendActivityAsync("Manca la sala");

            }
            else if(Date == DateTime.MinValue)
            {
                await stepContext.Context.SendActivityAsync(_intent.EntityNeedResponse);
                // ask for a date
            }
            else
            {
                // Create the reservation and sends a messagge tu the user
                await stepContext.Context.SendActivityAsync("Sala Riunioni Riservata" + " " + Date.ToShortDateString() + " " + Sala);
                // * TODO: Implementare in GraphAPIHelper metodo per prenotare una sala riunioni
            }

            return await stepContext.EndDialogAsync();
        }


        private Task<DialogTurnResult> ConfermaSalaAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private void ParseEntity()
        {
            foreach(var e in _intent.EntitiesCollected)
            {
  
                if (SaleRiunioniSet.Contains(e.Text))
                {
                    Sala = e.Text;
                }

                if ( e.Type.Equals(Datetime))
                {
                    Date = DateTime.Parse(e.Text);
                }
            }
        }

    }
}