using Botler.Models;
using Botler.Middleware.Services;
using Botler.Helpers;
using Botler.Dialogs.Dialoghi;
using static Botler.Dialogs.Utility.IntentNeedsEntityPhrases;
namespace Botler.Builders
{
    public class RichiestaSupportoIntentBuilder : IIntentBuilder
    {
        public Intent BuildIntent(LuisServiceResult luisServiceResult)
        {
            Intent intent = new GenericIntentBuilder().BuildIntent(luisServiceResult);

            intent.NeedEntities = true;
            intent.EntityLimit = 6;
            intent.EntityLowerBound = 1;
            intent.DialogID = "CreaTicket";
            intent.EntityNeedResponse = SupportoEntityNeedsToCollect;

            return intent;


        }
    }
}