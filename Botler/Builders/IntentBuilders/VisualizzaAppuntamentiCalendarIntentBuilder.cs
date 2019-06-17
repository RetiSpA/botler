using Botler.Models;
using Botler.Middleware.Services;
using Botler.Dialogs.Dialoghi;

namespace Botler.Builders.IntentBuilders
{
    public class   VisualizzaAppuntamentiCalendar : IIntentBuilder
    {
        public Intent BuildIntent(LuisServiceResult luisServiceResult)
        {
            Intent intent = new GenericIntentBuilder().BuildIntent(luisServiceResult);
            intent.NeedEntities = true;
            intent.EntityLimit = 1;
            intent.EntityLowerBound = 0;
            intent.DialogID = nameof(VisualizzaAppuntamentiCalendar);
            return intent;
        }
    }
}