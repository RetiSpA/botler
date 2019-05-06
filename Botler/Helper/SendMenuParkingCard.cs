using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Activity = Microsoft.Bot.Schema.Activity;
using static Botler.Dialogs.Utility.LuisIntent;

namespace Botler.Controller
{
    public class SendMenuParkingCard : SendHeroCard
    {
        public override Attachment CreateAttachement()
        {
            var heroCard = new HeroCard()
            {
                Title = "Botler - Parcheggio Aziandale -",
                Subtitle = "Il ChatBot di Reti S.p.A",
                Buttons = new List<CardAction> {

                    new CardAction(ActionTypes.PostBack, "Prenota posto auto", value: PrenotazioneIntent),
                    new CardAction(ActionTypes.PostBack, "Visualizza tempo a disposizione", value: TempoRimanentePrenotazioneIntent),
                    new CardAction(ActionTypes.PostBack, "Visualizza Prenotazione", value: VerificaPrenotazioneIntent),
                    new CardAction(ActionTypes.PostBack, "Cancella Prenotazione", value: CancellaPrenotazioneIntent),

                },
                Images = new List<CardImage> { new CardImage("https://i.pinimg.com/originals/0c/67/5a/0c675a8e1061478d2b7b21b330093444.gif")}

            };

            return heroCard.ToAttachment();
        }
    }
}