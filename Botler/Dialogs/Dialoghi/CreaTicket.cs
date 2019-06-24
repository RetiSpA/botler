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
using Botler.Controllers;
using Microsoft.Bot.Builder;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace Botler.Dialogs.Dialoghi
{
    public class CreaTicket : ComponentDialog
    {
        private const string CreaTicketDialog = "creaTicketDialog";
        private const string RispostaPrompt = "rispostaPrompt";

        private readonly Intent _intent;

        private readonly BotlerAccessors _accessors;

        public CreaTicket (Intent intent, BotlerAccessors accessors) : base(nameof(CreaTicket))
        {
            _intent = intent ?? throw new ArgumentNullException(nameof(intent));
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            _intent.EntitiesCollected = EntityFormatHelper.EntityFilterByIntent(_intent.Name, _intent.EntitiesCollected);

            ticket = new Ticket();

            var waterfallSteps = new WaterfallStep[]
            {
                CreaTicketAsync,
                PromptConfermaTicketAsync,
                PromptCreazioneTicketAsync,
                PromptPrioritaTicketAsync,
                PromptSalvaPrioritaAsync,
                DescrizioneTicketAsync,
                CreaTicketCompletoAsync,
            };

            AddDialog(new WaterfallDialog(CreaTicketDialog, waterfallSteps));
            AddDialog(new ChoicePrompt(RispostaPrompt));
            AddDialog(new TextPrompt("TextPrompt"));
        }

        private async Task<DialogTurnResult> PromptSalvaPrioritaAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userResponse = (stepContext.Result as FoundChoice).Value;

           ticket = await _accessors.TicketAccessors.GetAsync(stepContext.Context, () => new Ticket());

           if ( userResponse.Equals("normale"))
            {
                ticket.Priorita = 0;
            }

            if (userResponse.Equals("alta"))
            {
                ticket.Priorita = 1;
            }

            if (userResponse.Equals("urgente"))
            {
                ticket.Priorita = 2;
            }

            await _accessors.TicketAccessors.SetAsync(stepContext.Context, ticket);

            return await stepContext.NextAsync();

        }

        private async Task<DialogTurnResult> CreaTicketCompletoAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            ticket = await _accessors.TicketAccessors.GetAsync(stepContext.Context, () => new Ticket());
            ticket.Descrizione = stepContext.Context.Activity.Text;

            await _accessors.TicketAccessors.SetAsync(stepContext.Context, ticket);
            await _accessors.SaveStateAsync(stepContext.Context);

            var result = await TicketAPICaller.TicketPostAPIAsync(ticket);
            if (result)
            {
                await stepContext.Context.SendActivityAsync("Il ticket è stato creato con successo" + ticket.ToString());
                _intent.EntitiesCollected.Clear();
            }
            return await stepContext.EndDialogAsync();
        }

        private async Task<DialogTurnResult> PromptPrioritaTicketAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
           return await stepContext.PromptAsync(RispostaPrompt, new PromptOptions
            {
                Prompt = MessageFactory.Text("Scegli la priorità del tuo ticket"),
                Choices = ChoiceFactory.ToChoices(new List<string> { "Normale", "Alta", "Urgente"}),
            });
        }

        private async Task<DialogTurnResult> DescrizioneTicketAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync("TextPrompt", new PromptOptions
            {
                Prompt = MessageFactory.Text("Inserisci una descrizione del tuo problema"),
            });
        }

        private async Task<DialogTurnResult> PromptConfermaTicketAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(RispostaPrompt, new PromptOptions
            {
                Prompt = MessageFactory.Text("Ti apro un ticket per questa problematica.Vuoi procedere con una creazione dettagliata, o inviare così il ticket"),
                Choices = ChoiceFactory.ToChoices(new List<string> { "Più dettagli", "Crea"}),
            });
        }

        private async Task<DialogTurnResult> PromptCreazioneTicketAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userResponse = (stepContext.Result as FoundChoice).Value;
            ticket = await _accessors.TicketAccessors.GetAsync(stepContext.Context, () => new Ticket());

            if (userResponse.Equals("Crea") || userResponse.Equals("crea") || _intent.Name.Equals("ConfermaAzione"))
            {
                var result = await TicketAPICaller.TicketPostAPIAsync(ticket);
                if (result)
                {
                    await stepContext.Context.SendActivityAsync("Il ticket è stato creato con successo" + ticket.ToString());
                    _intent.EntitiesCollected.Clear();
                }
            }
           else
           {
               return await stepContext.NextAsync();
           }
           return await stepContext.EndDialogAsync();
        }

        public Ticket ticket { get; set; }

        private async Task<DialogTurnResult> CreaTicketAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            ParseEntity();

            var token = await _accessors.GetUserToken(stepContext.Context);
            var graphClient = GraphAPIHelper.GetGraphClient(token.Token);
            var me = await graphClient.Me.Request().GetAsync();

            ticket.Email_Utente = me.Mail;

            await _accessors.TicketAccessors.SetAsync(stepContext.Context, ticket);
            await _accessors.SaveStateAsync(stepContext.Context);

            if ( _intent.EntitiesCollected.Count < 2)
            {
                ticket.Descrizione = stepContext.Context.Activity.Text;
                return await stepContext.NextAsync();
            }
            else
            {
                var result = await TicketAPICaller.TicketPostAPIAsync(ticket);
                if (result)
                {
                    await stepContext.Context.SendActivityAsync("Ho abbastanza informazioni, il ticket è stato creato con successo" + ticket.ToString());
                    _intent.EntitiesCollected.Clear();
                }
               return await stepContext.EndDialogAsync();
            }
        }

        private void ParseEntity()
        {
            foreach(var e in _intent.EntitiesCollected)
            {
                var idTipoTicket = TicketSupportHelper.GetTipoTicket(e.Text);

                ticket.Categoria = e.Text;

                if (idTipoTicket > 0)
                {
                    ticket.ID_TipoTicket = (short) idTipoTicket;
                }

                if (e.Type.Equals(Descrizione))
                {
                    ticket.Descrizione = e.Text;
                }

                if (e.Type.Equals(Priorita))
                {
                    if ( e.Text.Equals("normale"))
                    {
                        ticket.Priorita = 0;
                    }

                    if (e.Text.Equals("alta"))
                    {
                        ticket.Priorita = 1;
                    }

                    else // urgente
                    {
                        ticket.Priorita = 2;
                    }
                }
            }

        }

    }
}