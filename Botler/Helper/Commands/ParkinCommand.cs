
using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Botler.Model;
using Botler.Controller;
using static Botler.Dialogs.Utility.Scenari;
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
        public async  Task ExecuteCommandAsync()
        {
            ISendAttachment send = new SendMenuParkingCard();
            await accessors.ScenarioStateAccessors.SetAsync(turn, Parking);
            await accessors.SaveStateAsync(turn);
            await send.SendAttachmentAsync(turn);
        }
    }
}