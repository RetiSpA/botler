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
                Images = new List<CardImage> { new CardImage("https://i.pinimg.com/originals/0c/67/5a/0c675a8e1061478d2b7b21b330093444.gif")}

            };

            return heroCard.ToAttachment();
        }
    }
}