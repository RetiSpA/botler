using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Botler.Helpers;
using Botler.Models;
using System.Text.RegularExpressions;
using static Botler.Dialogs.Utility.Responses;
using static Botler.Dialogs.Utility.ListsResponsesIT;
using static Botler.Dialogs.Utility.RegularExpressions;
using static Botler.Dialogs.Utility.LuisEntity;
using static Botler.Dialogs.Utility.EntitySets;

namespace Botler.Dialogs.Dialoghi
{
    public class CreaAppuntamentoCalendar : ComponentDialog
    {
        private const string CreaAppuntamentoCalendarDialog = "creaAppuntamentoCalendarDialog";

        private readonly Intent _intent;

        private readonly BotlerAccessors _accessors;

        public CreaAppuntamentoCalendar(Intent intent, BotlerAccessors accessors) : base(nameof(CreaAppuntamentoCalendar))
        {
            _intent = intent ?? throw new ArgumentNullException(nameof(intent));
            _intent.EntitiesCollected = EntityFormatHelper.EntityFilterByIntent(_intent.Name, _intent.EntitiesCollected);

            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
 
            Appuntamento = new AppuntamentoCalendar();

            var waterfallSteps = new WaterfallStep[]
            {
                CreaAppuntamentoAsync,
                MoreInfoAsync,
                // ConfermaAsync,
            };

            AddDialog(new WaterfallDialog(CreaAppuntamentoCalendarDialog, waterfallSteps));
        }

        private Task<DialogTurnResult> MoreInfoAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public AppuntamentoCalendar Appuntamento { get; set; }
        /// <summary>
        /// Check if all entity for create an appointment have been collected
        /// If all have been collected -> create a response and create an Outlook appoinment
        /// Else sends a message to the user for the missed entity
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> CreaAppuntamentoAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            EntityParse();
            var context = stepContext.Context;
            var token = await _accessors.GetUserToken(context);
            if (Appuntamento.Date == DateTime.MinValue)
            {
                await context.SendActivityAsync("Manca una data, inserisci una data valida per creare un appuntamento");
                return await stepContext.EndDialogAsync();
            }

            if (Appuntamento.Location == string.Empty)
            {
                await context.SendActivityAsync("Inserisci una location per creare un appuntamento");
                return await stepContext.EndDialogAsync();
            }

            await context.SendActivityAsync("Creato appuntamento: " + "\n Il giorno " + Appuntamento.Date.ToShortDateString() + " Orario : "
             + Appuntamento.Inizio.ToString() + " - " + Appuntamento.Fine.ToString() + " \nLocation: " + Appuntamento.Location);

            await GraphAPIHelper.CreateAppointmentAsync(context, token.Token, Appuntamento);
            return await stepContext.EndDialogAsync();
        }

        // TODO:
        private void EntityParse()
        {
            foreach (var e in _intent.EntitiesCollected)
            {
                if (e.Type.Equals(Datetime))
                {
                    Appuntamento.Date = DateTime.Parse(e.Text);
                }

                if (SaleRiunioniSet.Contains(e.Text))
                {
                    Appuntamento.Location = e.Text;
                }

                if (e.Type.Equals(Email))
                {
                    Appuntamento.Partecipanti.Add(e.Text);
                }

                if (e.Type.Equals(Time))
                {
                    if (Appuntamento.Inizio == TimeSpan.MinValue)
                    {
                        Appuntamento.Inizio = TimeSpan.Parse(e.Text);
                    }
                    else if (Appuntamento.Fine == TimeSpan.MinValue)
                    {
                        Appuntamento.Fine = TimeSpan.Parse(e.Text);
                    }
                }

                if (e.Type.Equals(TimeRegex))
                {
                    Regex regex = new Regex(RegexTimeFound_1);
                    var inizio = regex.Split(e.Text)[2];
                    var fine = regex.Split(e.Text)[5];

                    Appuntamento.Inizio = TimeSpan.Parse(inizio);
                    Appuntamento.Fine = TimeSpan.Parse(fine);
                }
  
            }
        }

    }
}