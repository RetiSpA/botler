using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Botler.Model;
using Botler.Controller;
using static Botler.Dialogs.Utility.Scenari;
using Botler.Helper.Attachment;

namespace Botler.Helper.Commands
{
    public class AreaRiservataCommand : ICommand
    {
        private ITurnContext turn;

        public AreaRiservataCommand(ITurnContext turn)
        {
            this.turn = turn ?? throw new ArgumentNullException(nameof(turn));
        }

        public async Task ExecuteCommandAsync()
        {
           ISendAttachment send = SendAttachmentFactory.FactoryMethod(MenuDipedenti);
           await send.SendAttachmentAsync(turn);
        }
    }
}