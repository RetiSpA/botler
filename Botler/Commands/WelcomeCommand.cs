using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Botler.Models;
using Botler.Controller;
using static Botler.Dialogs.Utility.Scenari;
using Botler.Helper.Attachment;

namespace Botler.Commands
{
    public class WelcomeCommand : ICommand
    {
        private ITurnContext _turn;

        public WelcomeCommand(){}
        public WelcomeCommand(ITurnContext turn)
        {
            _turn = turn ?? throw new ArgumentNullException(nameof(turn));
        }
 
        public async Task ExecuteCommandAsync()
        {
            ISendAttachment send = SendAttachmentFactory.FactoryMethod(Welcome);
            await send.SendAttachmentAsync(_turn);
        }
    }
}