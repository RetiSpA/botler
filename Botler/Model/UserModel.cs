using System;
using Microsoft.Bot.Schema;
using Microsoft.Graph;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Botler.Model;
using Botler.Helper.Commands;

namespace Botler.Dialogs.RisorseApi
{
    public class UserModel
    {
        public UserModel(){}
        public UserModel(TokenResponse token)
        {
             GraphUser = new GraphServiceClient(
                    new DelegateAuthenticationProvider(
                        requestMessage =>
                        {
                                // Append the access Token to the request.
                                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", token.Token);

                                // Get event times in the current time zone.
                                requestMessage.Headers.Add("Prefer", "outlook.timezone=\"" + TimeZoneInfo.Local.Id + "\"");

                            return Task.CompletedTask;
                        }));
        }

        public string Nome { get; set; }

        public string Cognome { get; set; }

        public string Email { get; set; }

        public int Id_Utente { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        public GraphServiceClient GraphUser { get; set; }

        public bool Autenticato { get; set; } = false;

        public TokenResponse Token { get; set; }

        public DateTime AuthTime { get; private set; }

        private const double authTimeOut = 30; // in minutes

        public async Task SaveUserDatesAsync(BotlerAccessors accessors, ITurnContext turn)
        {
            var me = await GraphUser.Me.Request().GetAsync();
            Nome = me.GivenName;
            Cognome = me.Surname;
            Autenticato = true;
            AuthTime = DateTime.Now;
        }

        public  bool  CheckAuthTimedOut()
        {
            if(Autenticato)
            {
                if((DateTime.Now.Minute - AuthTime.Minute) >= authTimeOut)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
