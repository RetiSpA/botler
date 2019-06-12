using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Botler.Builders;
using Botler.Helpers;
using Botler.Models;
using Microsoft.Bot.Builder;
using Newtonsoft.Json;

namespace Botler.Middleware.Services
{
    public class LuisServiceResult
    {


        public RecognizerResult LuisResult { get; set; }

        public Tuple<string,double> TopScoringIntent { get; set; }

        public IList<Entity> AllEntitiesFromLuis { get; set; } = new List<Entity> ();

        public Intent TopIntent { get; set; }

        public LuisServiceResult(RecognizerResult luisResult)
        {
            LuisResult = luisResult ?? throw new ArgumentNullException(nameof(luisResult));

            TopScoringIntent = LuisResult?.GetTopScoringIntent().ToTuple<string,double>();

            AddEntitiesFromLuisToList();

            TopIntent = IntentFactory.FactoryMethod(this);
        }

        public void AddEntitiesFromLuisToList()
        {
            var text = LuisResult.Text;
            var entities = LuisResult.Entities;
            var json = JsonConvert.SerializeObject(entities);
            AllEntitiesFromLuis = EntityFormatHelper.FormatEntityFromLuis(LuisResult);
        }
    }
}