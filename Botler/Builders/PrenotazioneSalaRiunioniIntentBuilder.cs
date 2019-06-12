using Botler.Models;
using Botler.Middleware.Services;
using Botler.Dialogs.Dialoghi;
using static Botler.Dialogs.Utility.IntentNeedsEntityPhrases;
namespace Botler.Builders
{
    public class PrenotazioneSalaRiunioniIntentBuilder : IIntentBuilder
    {
        public Intent BuildIntent(LuisServiceResult luisServiceResult)
        {
            Intent intent = new GenericIntentBuilder().BuildIntent(luisServiceResult);

            intent.NeedEntities = true;
            intent.EntityLimit = 4;
            intent.EntityLowerBound = 2;
            intent.DialogID = nameof(PrenotaSalaRiunioni);
            intent.EntityNeedResponse = PrenotaSalaEntityToCollect;

            return intent;
        }
    }
}