using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;
using Botler.Dialogs.Scenari;
using Botler.Dialogs.Dialoghi;
using Botler.Dialogs.RisorseApi;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Botler.Controller;
using Botler.Dialogs.Utility;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using static Botler.Dialogs.Utility.BotConst;
using static Botler.Dialogs.Utility.LuisIntent;
using static Botler.Dialogs.Utility.ListsResponsesIT;
using static Botler.Dialogs.Utility.Responses;
using System.IO;

namespace Botler.Dialogs.Scenari
{
    public class ScenarioController<T> where T : IScenario
    {
        private readonly ScenarioContainer _container;

        private readonly BotlerAccessors _accessors;

        private DialogContext CurrentDialogContext;

        private T CurrentScenario;

        private ITurnContext CurrentTurn;

        private LuisServiceResult LuisResult;

        private CancellationToken cancellationToken;

        public ScenarioController(BotlerAccessors accessors)
        {
            _accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));

            _container = new ScenarioContainer(_accessors);

            CurrentScenario = new DefaultScenario(_accessors);
        }

        public void InitTurnAccessors(ITurnContext CurrentTurn, LuisServiceResult LuisResult, CancellationToken cancellationToken)
        {
            this.CurrentTurn = CurrentTurn ?? throw new ArgumentNullException(nameof(CurrentTurn));
            this.LuisResult = LuisResult ?? throw new ArgumentNullException(nameof(LuisResult));
            this.cancellationToken = cancellationToken;

        }

        public async Task<string> CreateResponseForInterruptedStateAsync()
        {
            var topIntent = LuisResult.TopScoringIntent.Item1; // intent
            var score = LuisResult.TopScoringIntent.Item2; // score
            await CreateDialogContextFromScenarioAsync();

             if (topIntent.Equals(PresentazioneIntent) && (score > 0.75))
            {
                string response = (RandomResponses(PresentazioneResponse));

                return response;        // Handled the interrupt.
            }

            if (topIntent.Equals(GoodbyeIntent) && (score > 0.75))
            {
                string response = (RandomResponses(SalutoResponse));

                return response;        // Handled the interrupt.
            }

            if (topIntent.Equals(InformazioniIntent) && (score > 0.75))
            {
                string response = (RandomResponses(InformazioneResponse));

                return response;        // Handled the interrupt.
            }

            if (topIntent.Equals(RingraziamentiIntent) && (score > 0.75))
            {
                string response = (RandomResponses(RingraziamentoResponse));

                return response;        // Handled the interrupt.
            }

            if (topIntent.Equals(SalutePositivoIntent) && (score > 0.75))
            {
                string response = (RandomResponses(SalutoPositivoResponse));

                return response;        // Handled the interrupt.
            }

            if (topIntent.Equals(SaluteNegativoIntent) && (score > 0.75))
            {
                string response = (RandomResponses(SalutoNegativoResponse));

                return response;        // Handled the interrupt.
            }

            if (topIntent.Equals(AnomaliaIntent) && (score > 0.75))
            {
                string response = (RandomResponses(AnomaliaResponse));

                return response;        // Handled the interrupt.
            }

            if (topIntent.Equals(PossibilitàIntent) && (score > 0.75))
            {
                string response = (RandomResponses(PossibilitaParcheggioResponse));

                return response;        // Handled the interrupt.
            }

            return string.Empty;           // Did not handle
        }

        public async Task<DialogTurnResult> HandleDialogResultStatusAsync()
        {
            CurrentScenario = await CreateScenarioAsync();
             
            var dialogResult = await CurrentDialogContext.ContinueDialogAsync();

            switch (dialogResult.Status)
                {
                    case DialogTurnStatus.Empty:
                        return await StartDialog();

                    case DialogTurnStatus.Waiting:
                        // Do Nothing
                        break;
                    case DialogTurnStatus.Complete:
                        return await CurrentDialogContext.EndDialogAsync();

                    default:
                        return await CurrentDialogContext.CancelAllDialogsAsync();

                }
            return null;
        }

        private async Task<DialogTurnResult> StartDialog()
        {
            CurrentScenario = await CreateScenarioAsync();
            await CreateDialogContextFromScenarioAsync();
            if(CurrentScenario.GetType() == typeof(ParkingScenario))
            {
                return await StartParkingDialog();
            }

            return null;
        }

        private async Task<DialogTurnResult> StartParkingDialog()
        {
            var topIntent = LuisResult.TopScoringIntent.Item1;

            switch (topIntent)
            {
                case PrenotazioneIntent:
                    return await CurrentDialogContext.BeginDialogAsync(nameof(Prenotazione));

                case CancellaPrenotazioneIntent:
                    return await CurrentDialogContext.BeginDialogAsync(nameof(CancellaPrenotazione));

                case TempoRimanentePrenotazioneIntent:
                    return await CurrentDialogContext.BeginDialogAsync(nameof(VisualizzaTempo));

                case VerificaPrenotazioneIntent:
                    return await CurrentDialogContext.BeginDialogAsync(nameof(VisualizzaPrenotazione));

                case NoneIntent:
                    default:
                    return null;
            }
        }

        private async Task CreateDialogContextFromScenarioAsync()
        {

                DialogSet dialogSet = CurrentScenario.GetDialogSet();
                CurrentDialogContext = await dialogSet.CreateContextAsync(CurrentTurn);

        }

        private async Task<IScenario> CreateScenarioAsync()
        {
            IScenario newScenario = null;
            CurrentScenario = await _accessors.ScenarioStateAccessors.GetAsync(CurrentTurn, ScenarioFactory, cancellationToken);
            Console.WriteLine(CurrentScenario.ToString());
            string topEntity = String.Empty;
            JObject Entities = LuisResult.LuisResult.Entities;

            foreach (var ent in Entities)
            {
                var parking = ent.Key.ToString();

                 if(parking.Equals("Parking"))
                 {
                     //newScenario = new ParkingScenario(_accessors);

                    await _accessors.ScenarioStateAccessors.SetAsync(CurrentTurn, new ParkingScenario(_accessors));
                    await _accessors.SaveConvStateAsync(CurrentTurn);
                 }

            }

            return await _accessors.ScenarioStateAccessors.GetAsync(CurrentTurn, ScenarioFactory, cancellationToken);
        }

        private IScenario ScenarioFactory()
        {
            if (CurrentScenario.GetType() == typeof(DefaultScenario))
            {
                return new DefaultScenario(_accessors);
            }

            if (CurrentScenario.GetType() == typeof(ParkingScenario))
            {
                return new ParkingScenario(_accessors);
            }

            return new DefaultScenario(_accessors);

        }

        public Attachment CreateWelcomeCard()
        {
            return CreateAdaptiveCardAttachment();
        }
      
        private Attachment CreateAdaptiveCardAttachment()
        {
            var adaptiveCard = File.ReadAllText(@".\Dialogs\Welcome\Resources\welcomeCard.json");
            return new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCard),
            };
        }

        public async Task RepromptLastActivityDialogAsync()
        {
                if (CurrentDialogContext.ActiveDialog != null)
                {
                    await  CurrentDialogContext.RepromptDialogAsync();
                }
        }
    }
}