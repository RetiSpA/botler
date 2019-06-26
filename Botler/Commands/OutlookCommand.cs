
using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using static Botler.Dialogs.Utility.ListsResponsesIT;
using static Botler.Dialogs.Utility.Responses;

namespace Botler.Commands
{
    public class OutlookCommand : ICommand
    {
        private readonly ITurnContext _turn;

        public OutlookCommand(ITurnContext turn)
        {
            _turn = turn ?? throw new ArgumentNullException(nameof(turn));
        }
        public async Task ExecuteCommandAsync()
        {
            await _turn.SendActivityAsync("Con Outlook puoi:\n - Leggere le mail in una data specifica su Outllook \n - Creare e Visualizzare appuntamento su Outlook Calendar");
        }
    }
}