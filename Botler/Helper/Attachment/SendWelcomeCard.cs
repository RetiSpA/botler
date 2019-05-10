using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Activity = Microsoft.Bot.Schema.Activity;
using static Botler.Dialogs.Utility.Scenari;

namespace Botler.Controller
{
    public class SendWelcomeCard : SendHeroCard
    {
        public override Attachment CreateAttachement()
        {
            var heroCard = new HeroCard()
            {
                Title = "Botler",
                Subtitle = "Ciao sono Botler il ChatBot di Reti S.p.A, ecco qualche argomento per iniziare:",
                Buttons = new List<CardAction> {

                    new CardAction(ActionTypes.OpenUrl, "Pagina principale del sito", value: "https://www.reti.it/"),
                    new CardAction(ActionTypes.OpenUrl, "Contattaci per qualsiasi curiosità", value: "https://www.reti.it/contattaci/"),
                    new CardAction(ActionTypes.OpenUrl, "Rimani aggiornato consultando il nostro blog", value: "https://www.reti.it/blog/"),
                    new CardAction(ActionTypes.PostBack, "F.A.Q Reti S.p.A", value: QnAPublic),
                    new CardAction(ActionTypes.PostBack, "Clicca qui per accedere all'Area Riservata Reti S.p.A", value: Autenticazione)
                },
                Images = new List<CardImage> { new CardImage("https://i.pinimg.com/originals/0c/67/5a/0c675a8e1061478d2b7b21b330093444.gif")}

            };

            return heroCard.ToAttachment();
        }
    }
}