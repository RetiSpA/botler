using Botler.Models;
using Botler.Middleware.Services;

namespace Botler.Builders
{
    public class PrenotazioneIntentBuilder : IIntentBuilder
    {
        public Intent BuildIntent(LuisServiceResult luisServiceResult)
        {
            Intent intent = new GenericIntentBuilder().BuildIntent(luisServiceResult);
            intent.NeedEntities = true;
            intent.EntityLimit = 1;
            intent.EntityLowerBound = 1;

            return intent;
        }
    }
}