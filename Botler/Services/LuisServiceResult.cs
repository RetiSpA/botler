using System;
using Microsoft.Bot.Builder;

namespace Botler.Services
{
    public class LuisServiceResult
    {
        public RecognizerResult LuisResult { get; set; }

        public Tuple<string,double> TopScoringIntent { get; set; }

    }
}