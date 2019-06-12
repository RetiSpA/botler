using System.Security.Authentication;
using System.Threading.Tasks;
using Botler.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Driver.GridFS;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Botler.Dialogs.Utility.Commands;
using static Botler.Dialogs.Utility.Scenari;
using static Botler.Dialogs.Utility.BotConst;
using System.Configuration;

namespace Botler.Middleware.Services
{
    public class MongoDBService
    {
        public MongoClient Client { get; set; }

        private IMongoDatabase database;

        public MongoDBService()
        {
            // string connectionString = ConfigurationManager.ConnectionStrings["MongoDBConnection"].ConnectionString;
            string connectionString
             = "mongodb://context-botler-db:m5cSuH779tHFemm1ylCdFrBU5Zp2EVxMp7KUXFlR8sjuIQTTmRo4JV47opACvbcsNK2goWzRkpz8HfIjMLCJQw==@context-botler-db.documents.azure.com:10255/?ssl=true&replicaSet=globaldb";
            MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(connectionString));
            settings.SslSettings = new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };
            Client = new MongoClient(settings);
            database = Client.GetDatabase(MongoDatabase);
        }

        public async Task InsertJSONContextDocAsync(BotStateContext botState)
        {
            var json = JsonConvert.SerializeObject(botState);
            var document = BsonSerializer.Deserialize<BsonDocument>(json);
            var collection = database.GetCollection<BsonDocument>(MongoDBCollection);
            await collection.InsertOneAsync(document);
        }

        public async Task<IList<BotStateContext>> GetAllBotStateByConvIDAsync(string convID)
        {
            var collection = database.GetCollection<BotStateContext>(MongoDBCollection);
            var query = from bs in collection.AsQueryable<BotStateContext>()
                        where bs.Conversation_ID.Equals(convID)
                        select bs;

            List<BotStateContext> list = await query.ToListAsync();
            list.Sort();
            list.Reverse();
            return list;
        }
        public void  InsertJSONContextDoc(BotStateContext botState)
        {
            var json = JsonConvert.SerializeObject(botState);
            var document = BsonSerializer.Deserialize<BsonDocument>(json);
            var collection = database.GetCollection<BsonDocument>(MongoDBCollection);
            collection.InsertOne(document);
        }

        public BotStateContext GetAllBotStateByConvID(string convID)
        {
            var collection = database.GetCollection<BotStateContext>(MongoDBCollection);
            var query = from bs in collection.AsQueryable<BotStateContext>()
                        where bs.Conversation_ID.Equals(convID)
                        select bs;

            List<BotStateContext> list = query.ToList();
            list.Sort();
            return list[list.Count - 1 ];
        }
    }
}