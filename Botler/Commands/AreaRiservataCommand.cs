using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Botler.Controller;
using Botler.Helpers;
using Botler.Helper.Attachment;
using static Botler.Dialogs.Utility.Scenari;
using static Botler.Dialogs.Utility.ListsResponsesIT;
using static Botler.Dialogs.Utility.Responses;

namespace Botler.Commands
{
    public class AreaRiservataCommand : ICommand
    {
        private readonly ITurnContext _turn;

        private readonly BotlerAccessors _accessors;

        public AreaRiservataCommand(ITurnContext turn, BotlerAccessors accessors)
        {
            _turn = turn ?? throw new ArgumentNullException(nameof(turn));
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
        }

        public async Task ExecuteCommandAsync()
        {
           var alreadyAuth = await AuthenticationHelper.UserAlreadyAuthAsync(_turn, _accessors);

           if (alreadyAuth)
            {
                ISendAttachment send = SendAttachmentFactory.FactoryMethod(MenuDipedenti);
                await send.SendAttachmentAsync(_turn);
                await _accessors.TurnOffQnAAsync(_turn);
            }
            else
            {
                await _turn.SendActivityAsync(RandomResponses(AutenticazioneNecessariaResponse));
            }
        }
    }
}