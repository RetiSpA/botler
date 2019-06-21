using System;
using System.Collections.Generic;
using Botler.Builders.IntentBuilders;
using Botler.Helpers;
using Botler.Models;
using Microsoft.Bot.Builder;
using Newtonsoft.Json;

namespace Botler.Middleware.Services
{
    public class LuisServiceResult
    {


        public LuisServiceResult()
        {
            AllEntitiesFromLuis = new List<Entity>();
            TopScoringIntent = new Tuple<string, double>(string.Empty, 0.00);
        }

        public RecognizerResult LuisResult { get; set; }

        public Tuple<string,double> TopScoringIntent { get; set; }

        public ICollection<Entity> AllEntitiesFromLuis { get; set; }

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