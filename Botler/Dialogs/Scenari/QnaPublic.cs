using System.Threading.Tasks;
using Botler.Services;
using Microsoft.Bot.Builder.Dialogs;

namespace Botler.Dialogs.Scenari
{
    public class QnaPublic : IScenario
    {
        public DialogSet GetDialogSet()
        {
            throw new System.NotImplementedException();
        }

        public Task<DialogTurnResult> HandleDialogResultStatusAsync(LuisServiceResult luisServiceResult)
        {
            throw new System.NotImplementedException();
        }

        public bool NeedAuthentication()
        {
            throw new System.NotImplementedException();
        }
    }
}