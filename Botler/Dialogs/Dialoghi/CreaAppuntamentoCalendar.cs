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
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System.Collections.Generic;

namespace Botler.Dialogs.Dialoghi
{
    public class CreaAppuntamentoCalendar : ComponentDialog
    {
        private const string CreaAppuntamentoCalendarDialog = "creaAppuntamentoCalendarDialog";
        private const string RispostaPrompt = "rispostaPromt";
        private const string DateTimePromptStep = "dateTimePrompt";
        private const string TextPromptStep = "textPromptStep";

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
                CreazioneAsync,
                DataEventoAsync,
                DescrizioneEventoAsync,
                OrarioEventoAsync,
                TitoloEventoAsync,
                CreaTitoloEventoAsync,
                EventoCreatoAsync,
            };

            AddDialog(new WaterfallDialog(CreaAppuntamentoCalendarDialog, waterfallSteps));
            AddDialog(new ChoicePrompt(RispostaPrompt));
            AddDialog(new DateTimePrompt(DateTimePromptStep));
            AddDialog(new TextPrompt(TextPromptStep));
        }
        private async Task<DialogTurnResult> CreazioneAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userResponse = (stepContext.Result as FoundChoice).Value;
            Appuntamento = await _accessors.AppuntamentoAccessors.GetAsync(stepContext.Context, () => new AppuntamentoCalendar());

            if (userResponse.Equals("Più dettagli"))
            {
                return await stepContext.NextAsync();
            }
            if (userResponse.Equals("Crea") || _intent.Name.Equals("ConfermaAzione"))
            {
                var context = stepContext.Context;
                var token = await _accessors.GetUserToken(context);
                 await context.SendActivityAsync("Creato appuntamento: " + "\n Il giorno " + Appuntamento.Date.ToShortDateString() + " Orario : "
                        + Appuntamento.Inizio.ToString() + " - " + Appuntamento.Fine.ToString() + " \nLocation: " + Appuntamento.Location + "Tutto il giorno " + Appuntamento.IsAllDay);

                _intent.EntitiesCollected.Clear();
                await _accessors.AppuntamentoAccessors.SetAsync(stepContext.Context, Appuntamento);
                await _accessors.SaveStateAsync(stepContext.Context);
                await GraphAPIHelper.CreateAppointmentAsync(context, token.Token, Appuntamento);
                return await stepContext.EndDialogAsync();
            }

            return await stepContext.EndDialogAsync();
        }

        private async Task<DialogTurnResult> EventoCreatoAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var context = stepContext.Context;
            Appuntamento = await _accessors.AppuntamentoAccessors.GetAsync(context, () => new AppuntamentoCalendar());
            var token = await _accessors.GetUserToken(context);

            EntityParse();

            foreach(var email in Appuntamento.Partecipanti)
            {
                await context.SendActivityAsync(email);
            }
            await context.SendActivityAsync("Creato appuntamento: " + "\n Il giorno " + Appuntamento.Date.ToShortDateString() + " Orario : "
             + Appuntamento.Inizio.ToString() + " - " + Appuntamento.Fine.ToString() + " \nLocation: " + Appuntamento.Location + "Tutto il giorno " + Appuntamento.IsAllDay);

            _intent.EntitiesCollected.Clear();
            await GraphAPIHelper.CreateAppointmentAsync(context, token.Token, Appuntamento);
            return await stepContext.EndDialogAsync();
        }

        private async Task<DialogTurnResult> CreaTitoloEventoAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            Appuntamento = await _accessors.AppuntamentoAccessors.GetAsync(stepContext.Context, () => new AppuntamentoCalendar());

            var titolo = stepContext.Context.Activity.Text;
            Appuntamento.Titolo = titolo;

            await _accessors.AppuntamentoAccessors.SetAsync(stepContext.Context, Appuntamento);
            await _accessors.SaveStateAsync(stepContext.Context);

            return await stepContext.NextAsync();
        }

        /// <summary>
        /// This step check if in the previous one was insert a valid date or "AllDay" string for a all day event,
        /// and then prompt a request for a Title.
        /// </summary>
        /// <param name="stepContext"> </param>
        /// <param name="cancellationToken"> </param>
        /// <returns> </returns>
        private async Task<DialogTurnResult> TitoloEventoAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            Appuntamento = await _accessors.AppuntamentoAccessors.GetAsync(stepContext.Context, () => new AppuntamentoCalendar());
            var orario = stepContext.Context.Activity.Text;

            if (orario.Equals("AllDay") || orario.Equals("All Day"))
            {
                Appuntamento.IsAllDay = true;
            }
            else // try to format a date from user's text
            {
                EntityParse();
            }

            await _accessors.AppuntamentoAccessors.SetAsync(stepContext.Context, Appuntamento);
            await _accessors.SaveStateAsync(stepContext.Context);

            if (Appuntamento.Titolo == string.Empty)
            {
                return await stepContext.PromptAsync(TextPromptStep, new PromptOptions
                {
                    Prompt = MessageFactory.Text("Inserisci un titolo "),
                });
            }
            else
            {
                return await stepContext.NextAsync();
            }
        }

        private async Task<DialogTurnResult> OrarioEventoAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            Appuntamento = await _accessors.AppuntamentoAccessors.GetAsync(stepContext.Context, () => new AppuntamentoCalendar());

            if (Appuntamento.Inizio == TimeSpan.Zero && Appuntamento.Fine == TimeSpan.Zero)
            {
                var descrizione = stepContext.Context.Activity.Text;
                Appuntamento.Descrizione = descrizione;
                await _accessors.AppuntamentoAccessors.SetAsync(stepContext.Context, Appuntamento);
                await _accessors.SaveStateAsync(stepContext.Context);

                return await stepContext.PromptAsync(TextPromptStep, new PromptOptions
                {
                    Prompt = MessageFactory.Text("Inserisci un orario di inizio e fine, oppure scrivi 'AllDay' per creare un evento per tutta la giornata"),
                });
            }
            else
            {
                return await stepContext.NextAsync();
            }

        }

        private async Task<DialogTurnResult> DescrizioneEventoAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            Appuntamento = await _accessors.AppuntamentoAccessors.GetAsync(stepContext.Context, () => new AppuntamentoCalendar());

            // Check if we not have a Datetime
            if (Appuntamento.Date == DateTime.MinValue)
            {
               EntityParse();
            }

            await _accessors.AppuntamentoAccessors.SetAsync(stepContext.Context, Appuntamento);
            await _accessors.SaveStateAsync(stepContext.Context);

            return await stepContext.PromptAsync(TextPromptStep, new PromptOptions
            {
                Prompt = MessageFactory.Text("Inserisci una descrizione per il tuo evento"),
            });
        }

        private async Task<DialogTurnResult> DataEventoAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            Appuntamento = await _accessors.AppuntamentoAccessors.GetAsync(stepContext.Context, () => new AppuntamentoCalendar());

            if (Appuntamento.Date == DateTime.MinValue)
            {
                return await stepContext.PromptAsync(DateTimePromptStep, new PromptOptions
                {
                    Prompt = MessageFactory.Text("Inserisci una data per il tuo evento"),
                });

            }
            else
            {
                return await stepContext.NextAsync();
            }

        }

        private async Task<DialogTurnResult> MoreInfoAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(RispostaPrompt, new PromptOptions
            {
                Prompt = MessageFactory.Text("Vuoi creare l'evento con le informazioni che hai inserito o vuoi inserire più dettagli?"),
                Choices = ChoiceFactory.ToChoices(new List<string> { "Più dettagli", "Crea"}),
            });
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
            Console.WriteLine("Crea appuntamento");
            await _accessors.AppuntamentoAccessors.SetAsync(stepContext.Context, Appuntamento);
            await _accessors.SaveStateAsync(stepContext.Context);

            if (_intent.EntitiesCollected.Count < 4)
            {
                Console.WriteLine("Crea appuntamento next");
                return await stepContext.NextAsync();
            }
            else
            {
                await context.SendActivityAsync("Creato appuntamento: " + "\n Il giorno " + Appuntamento.Date.ToShortDateString() + " Orario : "
             + Appuntamento.Inizio.ToString() + " - " + Appuntamento.Fine.ToString() + " \nLocation: " + Appuntamento.Location + "Tutto il giorno " + Appuntamento.IsAllDay);

             _intent.EntitiesCollected.Clear();

            await GraphAPIHelper.CreateAppointmentAsync(context, token.Token, Appuntamento);
            return await stepContext.EndDialogAsync();
            }

        }

        private void EntityParse()
        {
            foreach (var e in _intent.EntitiesCollected)
            {
                if (e.Type.Equals(Datetime))
                {
                    var date  = DateTime.Parse(e.Text);
                    Appuntamento.Date = date;
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
                    var inizio = TimeSpan.MinValue;
                    var fine = TimeSpan.MinValue;

                    if (Appuntamento.Inizio == TimeSpan.MinValue)
                    {
                        inizio = TimeSpan.Parse(e.Text);
                    }
                    else if (Appuntamento.Fine == TimeSpan.MinValue)
                    {
                        fine  = TimeSpan.Parse(e.Text);

                        if (Appuntamento.Inizio < Appuntamento.Fine)
                        {
                            Appuntamento.Inizio = inizio;
                            Appuntamento.Fine = fine;
                            Appuntamento.IsAllDay  = false;
                        }
                    }

                }

                if (e.Type.Equals(TimeRegex))
                {
                    Regex regex = new Regex(RegexTimeFound_1);
                    var inizioRegex = regex.Split(e.Text)[2];
                    var fineRegex = regex.Split(e.Text)[5];

                    var inizio = TimeSpan.Parse(inizioRegex);
                    var fine = TimeSpan.Parse(fineRegex);

                    Appuntamento.Inizio = inizio;
                    Appuntamento.Fine = fine;
                    Appuntamento.IsAllDay  = false;
                }

            }
        }

    }
}