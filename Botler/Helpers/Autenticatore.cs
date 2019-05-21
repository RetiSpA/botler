using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Resources;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Net.Http;
using Microsoft.Bot.Builder.AI.QnA;
using System.Threading.Tasks;
using Botler.Dialogs.Dialoghi;
using Botler.Dialogs.RisorseApi;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Botler.Dialogs.Utility;
using Botler;
using Newtonsoft.Json.Linq;
using Botler.Controller;
using Microsoft.Graph;
using static Botler.Dialogs.Utility.BotConst;
using System.Net.Http.Headers;

namespace Botler.Controller
{

    public class Autenticatore
    {
        private Regex magicCodeRegex = new Regex("\\d{6}");

        private readonly ITurnContext _turn;

        public Autenticatore(){}

        public Activity CreateOAuthCard(ITurnContext turn)
        {
            var response = turn.Activity.CreateReply();

            response.Attachments.Add(new Microsoft.Bot.Schema.Attachment
            {
                ContentType = OAuthCard.ContentType,
                Content = new OAuthCard
                {
                    Text = "Clicca qui per procedere con l'autenticazione:",
                    ConnectionName = ConnectionName,
                    Buttons = new[]
                    {
                        new CardAction
                        {
                            Title = "Sign In",
                            Text = "Sign In",
                            Type = ActionTypes.Signin,
                        },
                    },
                },
            });
            return response;
        }

        public bool MagicCodeFound(string magicCode)
        {
            if (magicCode is null) return false;
            var matched = magicCodeRegex.Match(magicCode);
            return matched.Success;
        }

        public async Task<TokenResponse> RecognizeTokenAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            var adapter = (BotFrameworkAdapter) turnContext.Adapter;

            if (IsTokenResponseEvent(turnContext))
            {
                // The bot received the token directly
                var tokenResponseObject = turnContext.Activity.Value as JObject;
                var token = tokenResponseObject?.ToObject<TokenResponse>();
                return token;
            }

            // Microsoft Team:
            else if (IsTeamsVerificationInvoke(turnContext))
            {
                var magicCodeObject = turnContext.Activity.Value as JObject;
                var magicCode = magicCodeObject.GetValue("state")?.ToString();

                var token = await adapter.GetUserTokenAsync(turnContext, ConnectionName, magicCode, cancellationToken).ConfigureAwait(false);
                return token;
            }
            // MagicCode needed for authentication
            else if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                // make sure it's a 6-digit code
                var matched = magicCodeRegex.Match(turnContext.Activity.Text);

                if (matched.Success)
                {
                    TokenResponse token = await adapter.GetUserTokenAsync(turnContext, ConnectionName, matched.Value, cancellationToken).ConfigureAwait(false);
                    return token;
                }
            }
            return null;
        }

        public async static Task<string> GetQnAKeyFromAuthUser(ITurnContext turn, BotlerAccessors accessors)
        {
            bool alreadyAuth = await Autenticatore.UserAlreadyAuthAsync(turn, accessors);

            if(alreadyAuth)
            {
                return QnAKey;
            }
            else
            {
                return QnAPublicKey;
            }
        }

        public async static Task<bool> UserAlreadyAuthAsync(ITurnContext turnContext, BotlerAccessors accessors)
        {
            UserModel userAuthenticated = await accessors.GetAuthenticatedMemberAsync(turnContext);

            if(userAuthenticated != null)
            {
                bool authTimedOut = userAuthenticated.CheckAuthTimedOut(turnContext,accessors);

                if(authTimedOut)
                {
                   await LogOutUserAsync(turnContext, accessors);

                   return false;
                }

                else
                {
                    return userAuthenticated.Autenticato;
                }
            }

            else
            {
                return false;
            }
        }

        public async static Task LogOutUserAsync(ITurnContext turnContext, BotlerAccessors accessors)
        {
            var adapter = (BotFrameworkAdapter) turnContext.Adapter;
            await adapter.SignOutUserAsync(turnContext, ConnectionName);
            string memberID = turnContext.Activity.From.Id;
            List<UserModel> userList = await accessors.UserModelAccessors.GetAsync(turnContext, () => new List<UserModel>());
            UserModel userToRemove = null;
            foreach(UserModel user in userList)
            {
                if(user.Id_Utente.Equals(memberID))
                {
                   userToRemove = user;
                }
            }
            userList.Remove(userToRemove);
            await accessors.UserModelAccessors.SetAsync(turnContext, userList);
            await accessors.SaveStateAsync(turnContext);
        }

        private bool IsTokenResponseEvent(ITurnContext turnContext)
        {
            var activity = turnContext.Activity;
            return activity.Type == ActivityTypes.Event && activity.Name == "tokens/response";
        }

        private bool IsTeamsVerificationInvoke(ITurnContext turnContext)
        {
            var activity = turnContext.Activity;
            return activity.Type == ActivityTypes.Invoke && activity.Name == "signin/verifyState";
        }
    }
}