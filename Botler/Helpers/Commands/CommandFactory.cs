using System;
using System.Collections.Generic;
using Botler.Model;
using Microsoft.Bot.Builder;
using Newtonsoft.Json.Linq;
using static Botler.Dialogs.Utility.Commands;

namespace Botler.Helper.Commands
{
    public class CommandFactory
    {
        public static ICommand FactoryMethod(ITurnContext turn, BotlerAccessors accessors, string  entity)
        {
            if(AuthCommandFound(entity))  return new AuthenticationCommand(turn, accessors);

            if(ParkingCommandFound(entity)) return new ParkingCommand(turn, accessors);

            if(WelcomeCommandFound(entity)) return new WelcomeCommand(turn);

            if(AreaRiservataCommandFound(entity)) return new AreaRiservataCommand(turn, accessors);

            if(QnACommandFound(entity)) return new QnACommand(accessors, turn);

            if(QnAPublicCommandFound(entity)) return new QnAPublicCommand(accessors, turn);

            if(QnARiservataCommandFound(entity)) return new QnARiservataCommand(accessors, turn);

            if(LogoutCommandFound(entity)) return new LogoutCommand(turn, accessors);

            if(ExitCommandFound(entity)) return new ExitCommand(turn, accessors);

            if(HelpCommandFound(entity)) return new HelpCommand(turn);

            return null;
        }

        private static bool HelpCommandFound(string ent)
        {
            return ent.Equals(CommandHelp);
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

        private static bool QnACommandFound(string ent)
        {
            return ent.Equals(CommandQnA);
        }

        private static bool QnAPublicCommandFound(string ent)
        {
            return ent.Equals(CommandQnAPublic);
        }

        private static bool QnARiservataCommandFound(string ent)
        {
            return ent.Equals(CommandQnARiservata);
        }

        private static bool ExitCommandFound(string ent)
        {
            return ent.Equals(CommandExit);
        }

        private static bool LogoutCommandFound(string ent)
        {
            return ent.Equals(CommandLogout);
        }
    }
}