
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

namespace Botler.Model
{
    public class AuthenticationCommand : ICommand
    {
        private ITurnContext turn;

        private BotlerAccessors accessors;

        private Autenticatore autenticatore;

        public AuthenticationCommand(ITurnContext turn, BotlerAccessors accessors)
        {
            this.turn = turn ?? throw new ArgumentNullException(nameof(turn));
            this.accessors = accessors ?? throw new ArgumentNullException(nameof(turn));
            autenticatore = new Autenticatore();
        }

        public async Task ExecuteCommandAsync()
        {
            var message = turn.Activity.Text;
            await accessors.ScenarioStateAccessors.SetAsync(turn, Autenticazione);
            await accessors.SaveStateAsync(turn);
            var alreadyAuth = await Autenticatore.UserAlreadyAuth(turn, accessors);

            if(alreadyAuth)
            {
                await ChangesAndSaveStateAsync();
                await turn.SendActivityAsync(RandomResponses(AutenticazioneEffettuata));
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
            await accessors.ScenarioStateAccessors.SetAsync(turn, Autenticazione);
            Activity card = autenticatore.CreateOAuthCard(turn);
            await turn.SendActivityAsync(card).ConfigureAwait(false);
        }

        private async Task SecondPhaseAuthAsync()
        {
            var adapter = (BotFrameworkAdapter) turn.Adapter;
            var message = turn.Activity.AsMessageActivity();
            var response = string.Empty;

            var tokenResponse = autenticatore.RecognizeTokenAsync(turn, adapter);

                if (tokenResponse != null) // Autenticazione Succeded
                {
                    // Changes the CurrentDialog
                    await ChangesAndSaveStateAsync();

                    // Sends  a success response to the user
                    await  turn.SendActivityAsync(RandomResponses(AutenticazioneSuccessoResponse));

                    ICommand areaRiservata = CommandFactory.FactoryMethod(turn, accessors, CommandAreaRiservata);
                    await  areaRiservata.ExecuteCommandAsync();
                }

                else
                {
                    await turn.SendActivityAsync(RandomResponses(AnomaliaResponse));
                }
        }

       private async Task ChangesAndSaveStateAsync()
       {
            await accessors.AutenticazioneDipedenteAccessors.SetAsync(turn, true);
            await accessors.ScenarioStateAccessors.SetAsync(turn, Default);
            await accessors.SaveStateAsync(turn);
       }
    }
}