using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Botler.Helpers;
using Botler.Models;
using Newtonsoft.Json.Linq;
using static Botler.Dialogs.Utility.LuisEntity;
using static Botler.Dialogs.Utility.IntentsSets;
using static Botler.Dialogs.Utility.EntitySets;
using static Botler.Dialogs.Utility.LuisIntent;
using static Botler.Dialogs.Utility.RegularExpressions;
using Newtonsoft.Json;

namespace Botler.Builders
{
    public class EntityBuilder
    {
        private static Regex simpleEntity = new Regex(RegexDeserilizeEntityValue);

        public static Entity BuildEntityFromLUISResult(string luisJSON)
        {
            Entity entity = new Entity();
            var simpleEntityResult = simpleEntity.Split(luisJSON.ToString())[2];
            entity = JsonConvert.DeserializeObject<Entity>(simpleEntityResult);

            return entity;
        }
    }
}