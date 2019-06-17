using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Botler.Models;
using Botler.Controller;
using Botler.Helper.Attachment;
using Botler.Commands;
using Botler.Helpers;
using static Botler.Dialogs.Utility.Scenari;
using static Botler.Dialogs.Utility.ListsResponsesIT;
using static Botler.Dialogs.Utility.Responses;
using static Botler.Dialogs.Utility.Commands;

namespace Botler.Commands
{
    public class ParkingCommand : ICommand
    {
        private readonly ITurnContext _turn;

        private readonly BotlerAccessors _accessors;

        public ParkingCommand(ITurnContext turn, BotlerAccessors accessors)
        {
            this._turn = turn ?? throw new ArgumentNullException(nameof(turn));
            this._accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
        }

        public async Task ExecuteCommandAsync()
        {
            ISendAttachment send = SendAttachmentFactory.FactoryMethod(Parking);

            var alreadyAuth = await AuthenticationHelper.UserAlreadyAuthAsync(_turn, _accessors);

            if (!alreadyAuth)
            {
                await _turn.SendActivityAsync(RandomResponses(AutenticazioneNecessariaResponse));

                ICommand authCommand = CommandFactory.FactoryMethod(_turn, _accessors, CommandAuthentication);
                await authCommand.ExecuteCommandAsync();
                await _accessors.TurnOffQnAAsync(_turn);

                return;
            }

            await _accessors.SetCurrentScenarioAsync(_turn, Parking);
            await _accessors.SaveStateAsync(_turn);
            await send.SendAttachmentAsync(_turn);
        }
    }
}