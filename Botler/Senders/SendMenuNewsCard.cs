using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Activity = Microsoft.Bot.Schema.Activity;
using static Botler.Dialogs.Utility.Scenari;

namespace Botler.Controller
{
    public class SendMenuNewsCard : SendHeroCard
    {
        public override Attachment CreateAttachement()
        {
            var heroCard = new HeroCard()
            {
                Title = "Botler - News Aziendali -",
                Subtitle = "Il ChatBot di Reti S.p.A",
                Buttons = new List<CardAction> {

                    new CardAction(ActionTypes.PostBack, "Ultime Notizie", value: ""),
                    new CardAction(ActionTypes.PostBack, "Leggi l'ultimo numero del Magazine Reti", value: ""),
                    new CardAction(ActionTypes.PostBack, "Visualizza Calendario", value: ""),
                },

            };

            return heroCard.ToAttachment();
        }
    }
}