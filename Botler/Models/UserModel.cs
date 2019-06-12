using System;
using Microsoft.Bot.Schema;
using Microsoft.Graph;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Botler.Models;
using Botler.Commands;
using static Botler.Dialogs.Utility.BotConst;

namespace Botler.Dialogs.RisorseApi
{
    public class UserModel
    {

        private const double authTimeOut = 30; // minutes

        public UserModel() { }

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

        public string Id_Utente { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        public GraphServiceClient GraphUser { get; set; }

        public bool Autenticato { get; set; }

        public TokenResponse Token { get; set; }

        public DateTime AuthTime { get; set; }

        public string QnAKeyActive { get; set; }

        public async Task SaveUserDataAsync(BotlerAccessors accessors, ITurnContext turn, bool auth)
        {
            var me = await GraphUser.Me.Request().GetAsync();
            Nome = me.GivenName;
            Cognome = me.Surname;
            Email = Nome + "." + Cognome + "@reti.it";
            Id_Utente = turn.Activity.From.Id;
            Autenticato = auth;
            AuthTime = DateTime.Now;
            QnAKeyActive = QnAKey;
        }

        public bool CheckAuthTimedOut(ITurnContext turn, BotlerAccessors accessors)
        {
            if (Autenticato)
            {
                if (DateTime.Compare(DateTime.Now, AuthTime.Add(TimeSpan.FromMinutes(authTimeOut))) > 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
