
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Botler.Commands;
using Botler.Middleware.Services;

namespace Botler.Controller
{
    public static class CommandRecognizer
    {
      /// <summary>
      /// Find in LUIS any command entity, if it found one then it will be execute
      /// </summary>
      /// <param name="luisServiceResult"> LUIS RecognizerResult</param>
      /// <param name="accessors"></param>
      /// <param name="turn"></param>
      /// <returns> true if  only one command is found, false ohterwise </returns>
        public static async Task<bool> ExecutedCommandFromLuisResultAsync(LuisServiceResult luisServiceResult, BotlerAccessors accessors, ITurnContext turn)
        {
          var entities = luisServiceResult.LuisResult.Entities;
          var listent = luisServiceResult.LuisResult.Entities;

          IList<ICommand> listCommands = new List<ICommand>();
          ICommand command = null;
          string commandExecuted = string.Empty;

          foreach(var ent in entities)
          {
            command = CommandFactory.FactoryMethod(turn, accessors, ent.Key);

            if(command != null)
            {
              listCommands.Add(command);
            }

          }

            // We want to handle 1 command at time
          if (listCommands.Count != 1) return false;

          else
          {
              await listCommands[0].ExecuteCommandAsync();

              return true; // Command found and executed
          }
        }
    }
}
