using Botler.Models;
using Botler.Middleware.Services;
using Botler.Dialogs.Dialoghi;
using static Botler.Dialogs.Utility.IntentNeedsEntityPhrases;
namespace Botler.Builders.IntentBuilders
{
    public class CreaAppuntamentoCalendarIntentBuilder : IIntentBuilder
    {
        public Intent BuildIntent(LuisServiceResult luisServiceResult)
        {
            Intent intent = new GenericIntentBuilder().BuildIntent(luisServiceResult);

            intent.NeedEntities = true;
            intent.EntityLowerBound = 1;
            intent.EntityLimit = int.MaxValue;
            intent.DialogID = nameof(CreaAppuntamentoCalendar);
            intent.EntityNeedResponse = CreaAppuntamentoEntityNeedsToCollect;

            return intent;
        }
    }
}