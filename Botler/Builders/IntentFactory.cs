using Botler.Models;
using Botler.Middleware.Services;
using static Botler.Dialogs.Utility.LuisIntent;

namespace Botler.Builders
{
    public static class IntentFactory
    {
        public static Intent FactoryMethod(LuisServiceResult luisServiceResult)
        {
            var intent = luisServiceResult.TopScoringIntent.Item1;
            var score = luisServiceResult.TopScoringIntent.Item2;

            if (intent.Equals(LeggiMailIntent))
            {
                return new LeggiMailIntentBuilder().BuildIntent(luisServiceResult);
            }

            if (intent.Equals(PrenotazioneIntent))
            {
                return new PrenotazioneIntentBuilder().BuildIntent(luisServiceResult);
            }

            if (intent.Equals(PrenotazioneSalaRiunioniIntent))
            {
                return new PrenotazioneSalaRiunioniIntentBuilder().BuildIntent(luisServiceResult);
            }

            if (intent.Equals(VisualizzaEventiCalendarioIntent))
            {
                return new VisualizzaEventiCalendarioIntentBuilder().BuildIntent(luisServiceResult);
            }

            if (intent.Equals(RichiestaSupportoIntent))
            {
                return new RichiestaSupportoIntentBuilder().BuildIntent(luisServiceResult);
            }

            return new GenericIntentBuilder().BuildIntent(luisServiceResult);
        }
    }
}