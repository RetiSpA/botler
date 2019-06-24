using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Botler.Dialogs.RisorseApi;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using static Botler.Dialogs.Utility.BotConst;
using static Botler.Dialogs.Utility.Scenari;
using static Botler.Dialogs.Utility.Responses;
using static Botler.Dialogs.Utility.ListsResponsesIT;
using static Botler.Dialogs.Utility.Commands;
using Botler.Commands;
using Botler.Builders;
using Botler.Middleware.Services;
using Botler.Controller;

namespace Botler.Helpers
{
    public static class AuthenticationHelper
    {

        private static Regex magicCodeRegex = new Regex("\\d{6}");

        /// <summary>
        /// Create and send OAuth Card to the user
        /// </summary>
        /// <param name="turn"></param>
        /// <returns></returns>
        public static Activity CreateOAuthCard(ITurnContext turn)
        {
            var response = turn.Activity.CreateReply();

            response.Attachments.Add(new Microsoft.Bot.Schema.Attachment
            {
                ContentType = OAuthCard.ContentType,
                Content = new OAuthCard
                {
                    Text = RandomResponses(AutenticazioneNecessariaResponse),
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

        /// <summary>
        /// Checks if the user write a MagicCode
        /// </summary>
        /// <param name="magicCode"></param>
        /// <returns></returns>
        public static bool MagicCodeFound(string magicCode)
        {
            if (string.IsNullOrEmpty(magicCode)) return false;
            return magicCodeRegex.Match(magicCode).Success;
        }

        /// <summary>
        /// Validation user token
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<TokenResponse> RecognizeTokenAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
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

        /// <summary>
        /// Check if the user is auth or not and return the right QnA Maker API key
        /// </summary>
        /// <param name="turn"></param>
        /// <param name="accessors"></param>
        /// <returns></returns>
        public static async Task<string> GetQnAKeyFromAuthUser(ITurnContext turn, BotlerAccessors accessors)
        {
            bool alreadyAuth = await AuthenticationHelper.UserAlreadyAuthAsync(turn, accessors);

            if(alreadyAuth)
            {
                return QnAKey;
            }
            else
            {
                return QnAPublicKey;
            }
        }

        /// <summary>
        /// Check if the user is already auth in current conversation
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="accessors"></param>
        /// <returns></returns>
        public async static Task<bool> UserAlreadyAuthAsync(ITurnContext turnContext, BotlerAccessors accessors)
        {
            UserModel userAuthenticated = await accessors.GetAuthenticatedMemberAsync(turnContext);

            if (userAuthenticated != null)
            {
                bool authTimedOut = userAuthenticated.CheckAuthTimedOut(turnContext,accessors);

                if (authTimedOut)
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

        /// <summary>
        /// Logout user
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="accessors"></param>
        /// <returns></returns>
        public async static Task LogOutUserAsync(ITurnContext turnContext, BotlerAccessors accessors)
        {
            var adapter = (BotFrameworkAdapter) turnContext.Adapter;
            await adapter.SignOutUserAsync(turnContext, ConnectionName);
            string memberID = turnContext.Activity.From.Id;
            List<UserModel> userList = await accessors.UserModelAccessors.GetAsync(turnContext, () => new List<UserModel>());
            UserModel userToRemove = null;

            foreach (UserModel user in userList)
            {
                if (user.Id_Utente.Equals(memberID))
                {
                   userToRemove = user;
                }
            }

            userList.Remove(userToRemove);

            await accessors.UserModelAccessors.SetAsync(turnContext, userList);
            await accessors.SaveStateAsync(turnContext);
        }

        /// <summary>
        /// Init and Send the OAuth to the user
        /// </summary>
        /// <param name="turn"></param>
        /// <param name="accessors"></param>
        /// <returns></returns>
        public static async Task FirstPhaseAuthAsync(ITurnContext turn, BotlerAccessors accessors)
        {
            await InitAuthScenarioAsync(turn, accessors);

            await accessors.TurnOffQnAAsync(turn);
            await accessors.SetCurrentScenarioAsync(turn, Autenticazione);
            Activity card = AuthenticationHelper.CreateOAuthCard(turn);
            await turn.SendActivityAsync(card).ConfigureAwait(false);
        }

        /// <summary>
        /// In this phase we chek the token, if it a valid token,then save the user proprieties
        /// </summary>
        /// <param name="turn"></param>
        /// <param name="accessors"></param>
        /// <returns></returns>
        public  static async Task SecondPhaseAuthAsync(ITurnContext turn, BotlerAccessors accessors)
        {
            var message = turn.Activity.AsMessageActivity();
            var response = string.Empty;

            var tokenResponse = await AuthenticationHelper.RecognizeTokenAsync(turn);

                if (tokenResponse != null) // Autenticazione Succeded
                {
                    // Set in the authenticated user: memberID -> true
                   // await accessors.AddAuthenticatedUserAsync(turn);
                   // Create a user with MicrosoftGraphClient information
                    UserModel user = new UserModel(tokenResponse);
                   await user.SaveUserDataAsync( accessors, turn, true);

                    // Saves user in the Storage
                    await accessors.AddUserToAccessorsListAync(user, turn);

                    // Sends the operation succeded to the user
                    var randomResponse = RandomResponses(AutenticazioneSuccessoResponse);
                    await turn.SendActivityAsync(string.Format(randomResponse, user.Nome, user.Cognome));

                    // Now the user can use the Private QnA
                    await accessors.QnaActiveAccessors.SetAsync(turn, QnAKey);
                    await accessors.AddAuthUserAsync(turn, tokenResponse);
                    await accessors.SaveStateAsync(turn);

                    // Shows the Area Riservata menu
                    await ShowAreaRiservataAsync(turn, accessors);
                }
                else
                {
                    await turn.SendActivityAsync(RandomResponses(AutenticazioneErroreResponse));
                    var query = await accessors.GetLastUserQueryBeforeAuth(turn);
                    await turn.SendActivityAsync(query);
                }
        }
        
        /// <summary>
        /// Show the MenuDipendeti HeroCard
        /// </summary>
        /// <param name="turn"></param>
        /// <param name="accessors"></param>
        /// <returns></returns>
        private static async Task ShowAreaRiservataAsync(ITurnContext turn, BotlerAccessors accessors)
        {
            // await turn.SendActivityAsync(RandomResponses(AutenticazioneEffettuataResponse));
            ICommand commandAreaRiservata = CommandFactory.FactoryMethod(turn, accessors, CommandAreaRiservata);
            await commandAreaRiservata.ExecuteCommandAsync();
        }

        /// <summary>
        /// Init the MemoryStorage for the current conversation user
        /// </summary>
        /// <param name="turn"></param>
        /// <param name="accessors"></param>
        /// <returns></returns>
        private async static Task InitAuthScenarioAsync(ITurnContext turn, BotlerAccessors accessors)
        {
            await accessors.SetCurrentScenarioAsync(turn, Autenticazione);
            await accessors.SaveStateAsync(turn);
            await accessors.QnaActiveAccessors.SetAsync(turn, Default);
        }

        private static bool IsTokenResponseEvent(ITurnContext turnContext)
        {
            var activity = turnContext.Activity;
            return activity.Type == ActivityTypes.Event && activity.Name == "tokens/response";
        }

        private static bool IsTeamsVerificationInvoke(ITurnContext turnContext)
        {
            var activity = turnContext.Activity;
            return activity.Type == ActivityTypes.Invoke && activity.Name == "signin/verifyState";
        }
    }
}