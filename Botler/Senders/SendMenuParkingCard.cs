using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Dialogs;
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
                Title = "Botler - Parcheggio Aziendale  -",
                Subtitle = "Ecco le principali azioni previste per il Parcheggio Aziendale:",
                Buttons = new List<CardAction> {

                    new CardAction(ActionTypes.PostBack, "Prenota posto auto", value: PrenotazioneParcheggioIntent),
                    new CardAction(ActionTypes.PostBack, "Visualizza tempo a disposizione", value: TempoRimanentePrenotazioneParcheggioIntent),
                    new CardAction(ActionTypes.PostBack, "Visualizza Prenotazione", value: VerificaPrenotazioneParcheggioIntent),
                    new CardAction(ActionTypes.PostBack, "Cancella Prenotazione", value: CancellaPrenotazioneParcheggioIntent),

                },

            };

            return heroCard.ToAttachment();
        }
    }
}