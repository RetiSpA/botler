using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Botler.Model;
using Botler.Controller;
using static Botler.Dialogs.Utility.Scenari;

namespace Botler.Helper.Commands
{
    public class WelcomeCommand : ICommand
    {
         private ITurnContext turn;

        public WelcomeCommand(ITurnContext turn)
        {
            this.turn = turn ?? throw new ArgumentNullException(nameof(turn));
        }
        public async Task ExecuteCommandAsync()
        {
            ISendAttachment send = new SendWelcomeCard();
            await send.SendAttachmentAsync(turn);
        }

    }

}