using System.Collections.Generic;
using Botler.Model;
using Microsoft.Bot.Builder;
using Newtonsoft.Json.Linq;
using static Botler.Dialogs.Utility.Commands;

namespace Botler.Helper.Commands
{
    public class CommandFactory
    {
        public static ICommand FactoryMethod(ITurnContext turn, BotlerAccessors accessors,string  entity )
        {
            if(AuthCommandFound(entity))  return new AuthenticationCommand(turn, accessors);

            if(ParkingCommandFound(entity)) return new ParkingCommand(turn, accessors);

            if(WelcomeCommandFound(entity)) return new WelcomeCommand(turn);

            if(AreaRiservataCommandFound(entity)) return new AreaRiservataCommand(turn);

            return null;
        }

        private static bool AuthCommandFound(string ent)
        {
           return ent.Equals(CommandAuthentication);
        }

         private static bool ParkingCommandFound(string ent)
        {
            return ent.Equals(CommandParking);
        }

        private static bool WelcomeCommandFound(string ent)
        {
            return ent.Equals(CommandWelcome);
        }

        private static bool AreaRiservataCommandFound(string ent)
        {
          return ent.Equals(CommandAreaRiservata);
        }
    }
}