
using Botler.Model;
using Botler.Controller;
using Botler.Helper.Commands;
using Botler.Dialogs;
using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using static Botler.Dialogs.Utility.Commands;
using static Botler.Dialogs.Utility.Scenari;
using static Botler.Dialogs.Utility.Responses;
using static Botler.Dialogs.Utility.ListsResponsesIT;
using Botler.Dialogs.RisorseApi;
using Microsoft.Graph;
using System.Net.Http.Headers;

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
                    // Changes the CurrentDialog
                    await ChangesAndSaveStateAsync();


                // Sends  a success response to the user
                var randomResponse = RandomResponses(AutenticazioneSuccessoResponse);
                UserModel User = new UserModel(tokenResponse);
                await User.SaveUserDatesAsync(_accessors, _turn);
                await _accessors.UserModelAccessors.SetAsync(_turn, User); //Error 

                await _turn.SendActivityAsync(String.Format(randomResponse, User.Nome, User.Cognome));
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
            await _accessors.AutenticazioneDipedenteAccessors.SetAsync(_turn, true);
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