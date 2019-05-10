using System;
using System.Collections;
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
using static Botler.Dialogs.Utility.BotConst;

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

            response.Attachments.Add(new Attachment
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

        public async Task<TokenResponse> RecognizeTokenAsync(ITurnContext turnContext,  BotFrameworkAdapter adapter, CancellationToken cancellationToken = default(CancellationToken))
        {
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

        public async static Task<bool> UserAlreadyAuth(ITurnContext turnContext, BotlerAccessors accessors)
        {
            return await accessors.AutenticazioneDipedenteAccessors.GetAsync(turnContext, () => false);
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