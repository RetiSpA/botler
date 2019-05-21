
using Botler.Model;
using Botler.Controller;
using Botler.Helper.Commands;
using Botler.Dialogs;
using Botler.Dialogs.RisorseApi;
using Microsoft.Graph;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using static Botler.Dialogs.Utility.Commands;
using static Botler.Dialogs.Utility.Scenari;
using static Botler.Dialogs.Utility.Responses;
using static Botler.Dialogs.Utility.ListsResponsesIT;
using static Botler.Dialogs.Utility.BotConst;


namespace Botler.Model
{
    public class AuthenticationCommand : ICommand
    {
        private  readonly ITurnContext _turn;

        private readonly  BotlerAccessors _accessors;

        private Autenticatore autenticatore;

        public AuthenticationCommand(ITurnContext turn, BotlerAccessors accessors)
        {
            this._turn = turn ?? throw new ArgumentNullException(nameof(turn));
            this._accessors = accessors ?? throw new ArgumentNullException(nameof(_accessors));
            autenticatore = new Autenticatore();
        }

        public async Task ExecuteCommandAsync()
        {
            await InitAuthScenarioAsync();

            var message = _turn.Activity.Text;
            var alreadyAuth = await Autenticatore.UserAlreadyAuthAsync(_turn, _accessors);

            if(alreadyAuth)
            {
                await ChangesAndSaveStateAsync();
                await ShowAreaRiservataAsync();
                return;
            }
            if(autenticatore.MagicCodeFound(message))
            {
                await SecondPhaseAuthAsync();
            }

            else
            {
                await FirstPhaseAuthAsync();
            }
        }

        private async Task FirstPhaseAuthAsync()
        {
            await _accessors.TurnOffQnAAsync(_turn);
            await _accessors.SetCurrentScenarioAsync(_turn, Autenticazione);
            Activity card = autenticatore.CreateOAuthCard(_turn);
            await _turn.SendActivityAsync(card).ConfigureAwait(false);
        }

        private async Task SecondPhaseAuthAsync()
        {
            var message = _turn.Activity.AsMessageActivity();
            var response = string.Empty;

            var tokenResponse = await autenticatore.RecognizeTokenAsync(_turn);

                if (tokenResponse != null) // Autenticazione Succeded
                {
                    // Set in the authenticated user: memberID -> true
                   // await _accessors.AddAuthenticatedUserAsync(_turn);

                   // Create a user with MicrosoftGraphClient information
                    UserModel user = new UserModel(tokenResponse);
                    await user.SaveUserDataAsync( _accessors, _turn, true);

                    // Saves user in the Storage
                    await _accessors.AddUserToAccessorsListAync(user, _turn);

                    // Sends the operation succeded to the user
                    var randomResponse = RandomResponses(AutenticazioneSuccessoResponse);
                    await _turn.SendActivityAsync(String.Format(randomResponse, user.Nome, user.Cognome));

                    // Now the user can use the Private QnA
                    await _accessors.QnaActiveAccessors.SetAsync(_turn, QnAKey);

                    await _accessors.SaveStateAsync(_turn);

                    // Shows the Area Riservata menu
                    ICommand areaRiservata = CommandFactory.FactoryMethod(_turn, _accessors, CommandAreaRiservata);
                    await areaRiservata.ExecuteCommandAsync();
                }

                else
                {
                    await _turn.SendActivityAsync(RandomResponses(AutenticazioneErroreResponse));
                }
        }

        private async Task ChangesAndSaveStateAsync()
       {
            await _accessors.SetCurrentScenarioAsync(_turn, Default);
            await _accessors.SaveStateAsync(_turn);
       }

        private async Task InitAuthScenarioAsync()
       {
            await _accessors.SetCurrentScenarioAsync(_turn, Autenticazione);
            await _accessors.SaveStateAsync(_turn);
            await _accessors.QnaActiveAccessors.SetAsync(_turn, Default);
       }

        private async Task ShowAreaRiservataAsync()
       {
            await _turn.SendActivityAsync(RandomResponses(AutenticazioneEffettuataResponse));
            ICommand commandAreaRiservata = CommandFactory.FactoryMethod(_turn, _accessors, CommandAreaRiservata);
            await commandAreaRiservata.ExecuteCommandAsync();
       }
    }
}