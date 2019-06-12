using Botler.Models;
using Botler.Middleware.Services;

namespace Botler.Builders
{
    public class VisualizzaEventiCalendarioIntentBuilder : IIntentBuilder
    {
        public Intent BuildIntent(LuisServiceResult luisServiceResult)
        {
            Intent intent = new GenericIntentBuilder().BuildIntent(luisServiceResult);
            intent.NeedEntities = true;
            intent.EntityLimit = intent.EntityLowerBound = 2;

            return intent;
        }
    }
}