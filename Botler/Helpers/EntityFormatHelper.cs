using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Botler.Models;
using Newtonsoft.Json.Linq;
using Microsoft.Bot.Builder;
using Newtonsoft.Json;
using static Botler.Dialogs.Utility.LuisEntity;
using static Botler.Dialogs.Utility.IntentsSets;
using static Botler.Dialogs.Utility.EntitySets;
using static Botler.Dialogs.Utility.LuisIntent;
using static Botler.Dialogs.Utility.RegularExpressions;
using Botler.Builders;

namespace Botler.Helpers
{
    public class EntityFormatHelper
    {
        // TODO: Funzione molto lenta si potrebbe cambiare.
        public static IList<Entity> FormatEntityFromLuis(RecognizerResult luisResult)
        {
            IList<Entity> list = new List<Entity>();
            JObject entitiesFromLuis = luisResult.Entities;
            Regex simpleEntity = new Regex(RegexDeserilizeEntityValue);
            Regex complexEntity = new Regex(RegexDeserilizeComplexEntityValue);

            foreach(var ent in entitiesFromLuis)
            {
                // A entity value in LuisResult is this part of the JSON file
                //  "datetime_regex": [
                //   {
                //     "endIndex": 20,
                //     "startIndex": 0,
                //     "text": "si il 22 aprile 2019",
                //     "type": "datetime_regex"
                //   }
                // ],

                // Then we want to take only the "key":"value" useful to the Entity model.

                foreach(var v in ent.Value)
                {
                    if (simpleEntity.Split(v.ToString()).Length > 1)
                    {
                        Entity entity = EntityBuilder.BuildEntityFromLUISResult(v.ToString());

                        // A Textul date could be : 22 maggio or 01 aprile 2018 (e.g)
                        if (entity.Type.Equals(DatetimeBuiltin) || entity.Type.Equals(DatetimeTextual) || entity.Type.Equals(DatetimeRegex) || entity.Type.Equals(DatetimeRegexFormat))
                        {
                            // In this case we have a text is already in this format -> dd/mm/yyyy
                            if (entity.Type.Equals(DatetimeRegexFormat))
                            {
                                entity.Text = DateTime.Parse(entity.Text).ToShortDateString();
                            }
                            else
                            {
                                entity.Text = FormatDateTimeFromText(entity.Text);
                            }

                            entity.Type = Datetime;
                        }

                        if (entity.Text != string.Empty)
                        {
                            list.Add(entity);
                        }

                    }
                }
            }

            return list;
        }


        public static IList<Entity> FiltrEntityByIntent(string intentName, IList<Entity>  entitiesCollected)
        {
            IList<Entity> list = new List<Entity>();

            foreach(var ent in entitiesCollected)
            {
                if (OutlookIntents.Contains(intentName) && OutlookEntities.Contains(ent.Type))
                {
                    list.Add(ent);
                }

                if (ParkingIntents.Contains(intentName) && ParkingEntities.Contains(ent.Type))
                {
                    list.Add(ent);
                }
                // we dobule check here because we have a list of Synonyms entity in a entity list ( the text is a list's value (or his synonyms))
                if (SupportIntents.Contains(intentName) && SupportEntities.Contains(ent.Text) && SupportEntities.Contains(ent.Type))
                {
                    list.Add(ent);
                }
            }
            return list;
        }

        public static string FormatDateTimeFromText(string datetimeText)
        {
            Regex regexDatetimeText = new Regex(RegexFindDateTimeFromText);
            Regex regexYearInRange = new Regex(RegexCheckYear);

            int monthNumber;
            string day;
            string month;
            string year;
            string dateTimeString;
            DateTime dateTimeResult;

            if (regexDatetimeText.Match(datetimeText).Success)
            {
                day = regexDatetimeText.Split(datetimeText)[2];
                month = regexDatetimeText.Split(datetimeText)[4];
                year = regexDatetimeText.Split(datetimeText)[5];

                year = year.Replace(" ", string.Empty);

                if (MonthEntities.TryGetValue(month, out monthNumber))
                {
                    // Se non viene trovato un anno, usiamo come default l'anno corrente
                    if (!regexYearInRange.Match(year).Success)
                    {
                        year = DateTime.Now.Year.ToString();
                    }
                    // we are in a correct instance of datetime
                    dateTimeString = day + "/" + monthNumber + "/" + year;
                    try
                    {
                        dateTimeResult = DateTime.Parse(dateTimeString);
                    }
                    catch (Exception e)
                    {
                        return string.Empty;
                    }

                    return dateTimeResult.ToShortDateString();
                }
            }

            return string.Empty;
        }
    }
}