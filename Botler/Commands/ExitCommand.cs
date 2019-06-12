using System;
using System.Threading.Tasks;
using Botler.Models;
using Botler.Helper;
using Microsoft.Bot.Builder;
using static Botler.Dialogs.Utility.BotConst;
using static Botler.Dialogs.Utility.ListsResponsesIT;
using static Botler.Dialogs.Utility.Responses;
using static Botler.Dialogs.Utility.Scenari;
using Botler.Controller;
using Botler.Helper.Attachment;

namespace Botler.Commands
{
    public class ExitCommand : ICommand
    {
        private readonly ITurnContext _turn;

        private readonly BotlerAccessors _accessors;

        public ExitCommand(ITurnContext turn, BotlerAccessors accessors)
        {
            this._turn = turn ?? throw new ArgumentNullException(nameof(turn));
            this._accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
        }

        public async Task ExecuteCommandAsync()
        {
            await _accessors.ResetScenarioAsync(_turn);

            ISendAttachment send = SendAttachmentFactory.FactoryMethod(Welcome);
            await send.SendAttachmentAsync(_turn);
        }
    }
}