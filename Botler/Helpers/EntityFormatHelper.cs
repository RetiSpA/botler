using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Botler.Models;
using Newtonsoft.Json.Linq;
using Microsoft.Bot.Builder;
using static Botler.Dialogs.Utility.LuisEntity;
using static Botler.Dialogs.Utility.IntentsSets;
using static Botler.Dialogs.Utility.EntitySets;
using static Botler.Dialogs.Utility.RegularExpressions;
using Botler.Builders;
using System.Globalization;

namespace Botler.Helpers
{
    /// <summary>
    /// Format the entities coming from Luis to Models.Entity class
    /// </summary>
    public class EntityFormatHelper
    {
        /// <summary>
        ///  Read the Luis JSON result coming from actual turn, and find entities to format
        /// </summary>
        /// <param name="luisResult"></param>
        /// <returns></returns>
        public static ICollection<Entity> FormatEntityFromLuis(RecognizerResult luisResult)
        {
            ICollection<Entity> set = new List<Entity>();
            JObject entitiesFromLuis = luisResult.Entities;

            foreach (var ent in entitiesFromLuis)
            {  // Then we want to take only the "key":"value" useful to the Entity model.
                foreach (var v in ent.Value)
                {
                    var entityValue = v.ToString();

                    Entity entity = CreateEntityWithLuisJson(entityValue);

                    if (entity != null && entity.Text != string.Empty)
                    {
                        set.Add(entity);
                    }

                }
            }

            return set;
        }

        /// <summary>
        /// Create an instance of Models.Entity class, with the entity value collected from LUIS JSON
        /// </summary>
        /// <param name="entityValue"></param>
        /// <returns> a new instance of Entity</returns>
        private static Entity CreateEntityWithLuisJson(string entityValue)
        {
            Regex simpleEntity = new Regex(RegexDeserilizeEntityValue);
            // Valide entity found
            if (simpleEntity.Split(entityValue).Length > 1)

            {
                Entity entity = EntityBuilder.BuildEntityFromLUISResult(entityValue);

                // try to normalize a datetime types coming from luis:
                if (DatesEntitiesSet.Contains(entity.Type))
                {
                    entity = FormatDateTimeEntity(entity);
                }

                return entity;
            }

            return null;
        }

        /// <summary>
        /// Format the entity.Text and entity.Type of type DateTime
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private static Entity FormatDateTimeEntity(Entity entity)
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

            return entity;
        }

        /// <summary>
        /// Return a ICollection<Entity> with the only entities needed for the intent
        /// </summary>
        /// <param name="intentName"></param>
        /// <param name="entitiesCollected"></param>
        /// <returns></returns>
        public static ICollection<Entity> EntityFilterByIntent(string intentName, ICollection<Entity> entitiesCollected)
        {
            ISet<Entity> set = new HashSet<Entity>();

            foreach (var ent in entitiesCollected)
            {
                if (OutlookIntents.Contains(intentName) && OutlookEntities.Contains(ent.Type))
                {
                    set.Add(ent);
                }

                if (ParkingIntents.Contains(intentName) && ParkingEntities.Contains(ent.Type))
                {
                    set.Add(ent);
                }

                if (SupportIntents.Contains(intentName) && SupportEntities.Contains(ent.Text) && SupportEntities.Contains(ent.Type))
                {
                    set.Add(ent);
                }
            }

            return set;
        }

        /// <summary>
        /// Take a DateTime in textual form ( 22 luglio ) and format it  in MM/dd/yyyy
        /// </summary>
        /// <param name="datetimeText"></param>
        /// <returns></returns>
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
                    if (regexYearInRange.Match(year).Success == false)
                    {
                        year = DateTime.Now.Year.ToString();
                    }
                    // we are in a correct instance of datetime -> MM/dd/yyyy
                    dateTimeString =  monthNumber  + "/" +  day + "/" + year;

                    try
                    {
                        dateTimeResult = DateTime.ParseExact( dateTimeString, "M/dd/yyyy", CultureInfo.InvariantCulture);
                    }
                    catch (Exception e)
                    {
                        return string.Empty;
                    }

                    return dateTimeResult.ToShortDateString();
                }
            }

            if (datetimeText.Equals("oggi"))
            {
                return DateTime.Now.ToShortDateString();
            }

            if (datetimeText.Equals("domani"))
            {
                var today = DateTime.Now.AddDays(1);
                return today.ToShortDateString();
            }

            if (datetimeText.Equals("ieri"))
            {
                var today = DateTime.Now.AddDays(-1);
                return today.ToShortDateString();
            }
            return string.Empty;
        }

    }
}