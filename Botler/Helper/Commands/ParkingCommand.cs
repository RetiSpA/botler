using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Botler.Model;
using Botler.Controller;
using static Botler.Dialogs.Utility.Scenari;
using static Botler.Dialogs.Utility.ListsResponsesIT;
using static Botler.Dialogs.Utility.Responses;
using Botler.Helper.Attachment;

namespace Botler.Model
{
    public class ParkingCommand : ICommand
    {
        private ITurnContext turn;

        private BotlerAccessors accessors;

        public ParkingCommand(ITurnContext turn, BotlerAccessors accessors)
        {
            this.turn = turn ?? throw new ArgumentNullException(nameof(turn));
            this.accessors = accessors ?? throw new ArgumentNullException(nameof(turn));
        }

        public async Task ExecuteCommandAsync()
        {
            ISendAttachment send = SendAttachmentFactory.FactoryMethod(Parking);

            var alreadyAuth = await Autenticatore.UserAlreadyAuth(turn, accessors);

            if(!alreadyAuth)
            {
                await turn.SendActivityAsync(RandomResponses(AutenticazioneNecessariaResponse));
                return;
            }

            await accessors.ScenarioStateAccessors.SetAsync(turn, Parking);
            await accessors.SaveStateAsync(turn);
            await send.SendAttachmentAsync(turn);
        }
    }
}