using Botler.Models;
using Botler.Helpers;
using Botler.Middleware.Services;

namespace Botler.Builders.IntentBuilders
{
    public class GenericIntentBuilder : IIntentBuilder
    {
        public Intent BuildIntent(LuisServiceResult luisServiceResult)
        {
            Intent intent = new Intent();
            intent.Name = luisServiceResult.TopScoringIntent.Item1; // Intent
            intent.Score = luisServiceResult.TopScoringIntent.Item2; // Score
            intent.EntitiesCollected = luisServiceResult.AllEntitiesFromLuis;

            return intent;

        }
    }
}