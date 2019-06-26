using System;
using System.Collections.Generic;
using Botler.Dialogs.RisorseApi;
using Botler.Dialogs.Scenari;
using Botler.Models;
using MongoDB.Bson.Serialization.Attributes;
using static Botler.Dialogs.Utility.Scenari;

namespace Botler.Models
{
    [BsonIgnoreExtraElements]
    public class BotStateContext : IComparable
    {
        public BotStateContext ()
        {
            insertDate = DateTime.Now;
        }

        public DateTime insertDate { get; private set; }
        public string scenarioID { get; set; }

        public Intent TopIntent { get; set; }

        public string Conversation_ID { get; set; }

        public int Turn { get; set; }

        public string UserQuery { get; set; }

        public UserModel User { get; set; }

        public int CompareTo(object obj)
        {
            if (obj is null) return 1;

            BotStateContext botState = (BotStateContext) obj;

            return insertDate.CompareTo(botState.insertDate);
        }

        public override string ToString()
        {
            return scenarioID + " " + " " + Turn  + " " + UserQuery;
        }
    }
 }
