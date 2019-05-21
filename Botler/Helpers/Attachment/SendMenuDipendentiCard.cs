using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Activity = Microsoft.Bot.Schema.Activity;
using static Botler.Dialogs.Utility.Scenari;
using static Botler.Dialogs.Utility.Commands;

namespace Botler.Controller
{
    public class SendMenuDipendentiCard : SendHeroCard
    {
        public override Attachment CreateAttachement()
        {
            var heroCard = new HeroCard()
            {
                Title = "Botler - Area Riservata -",
                Subtitle = "Menu riservato ai dipedenti di Reti S.p.A, ecco le principali azioni:",
                Buttons = new List<CardAction> {

                    new CardAction(ActionTypes.PostBack, "Parcheggio Aziendale", value: Parking),
                    new CardAction(ActionTypes.PostBack, "QnA e F.A.Q Aziendale", value: QnA),
                   // new CardAction(ActionTypes.PostBack ,"News di Reti S.p.A", value: "News"),
                },
                //Images = new List<CardImage> { new CardImage("https://i.pinimg.com/originals/0c/67/5a/0c675a8e1061478d2b7b21b330093444.gif")}

            };

            return heroCard.ToAttachment();
        }
    }
}