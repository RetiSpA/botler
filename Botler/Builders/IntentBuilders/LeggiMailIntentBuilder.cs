using Botler.Models;
using Botler.Middleware.Services;
using Botler.Helpers;
using Botler.Dialogs.Dialoghi;
using static Botler.Dialogs.Utility.IntentNeedsEntityPhrases;

namespace Botler.Builders.IntentBuilders
{
    public class LeggiMailIntentBuilder : IIntentBuilder
    {
        public Intent BuildIntent(LuisServiceResult luisServiceResult)
        {
            Intent intent = new GenericIntentBuilder().BuildIntent(luisServiceResult);

            intent.NeedEntities = true;
            intent.EntityLimit = 2;
            intent.EntityLowerBound = 1;
            intent.DialogID = nameof(LetturaMailOutlook);
            intent.EntityNeedResponse = LeggiMailEntityNeedsToCollect;

            return intent;
        }
    }
}