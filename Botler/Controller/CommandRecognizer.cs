

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Botler.Model;
using Botler.Controller;
using Microsoft.Bot.Builder;
using Newtonsoft.Json.Linq;
using Botler.Helper.Commands;

namespace Botler.Controller
{
    public class CommandRecognizer
    {
        public static async Task<bool>  ExecutedCommandFromLuisResultAsync(LuisServiceResult LuisServiceResult, BotlerAccessors accessors, ITurnContext turn)
        {
          var entities = LuisServiceResult.LuisResult.Entities;
          IList<ICommand> listCommands = new List<ICommand>();
          ICommand command = null;

          foreach(var ent in entities)
          {
            command  = CommandFactory.FactoryMethod(turn, accessors, ent.Key);

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
