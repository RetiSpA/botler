using Botler.Middleware.Services;
using Botler.Models;

namespace Botler.Builders
{
    public interface IIntentBuilder
    {
        Intent BuildIntent(LuisServiceResult luisServiceResult);
    }
}