using System;
using Microsoft.Bot.Schema;
using Microsoft.Graph;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;

namespace Botler.Dialogs.RisorseApi
{
    public class UserModel
    {
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

        public GraphServiceClient GraphUser { get; set; }

        public bool Autenticato { get; set; } = false;

        public TokenResponse Token { get; set; }

        public async Task SaveUserDatesAsync(BotlerAccessors accessors, ITurnContext turn)
        {
            var me = await GraphUser.Me.Request().GetAsync();
            Nome = me.GivenName;
            Cognome = me.Surname;
            Autenticato = true;
            
        }
    }
}
