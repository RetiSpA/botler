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
using Microsoft.Bot.Builder.AI.QnA;
using System.Threading.Tasks;
using Botler.Dialogs.Dialoghi;
using Botler.Dialogs.RisorseApi;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Botler.Dialogs.Utility;
using Botler;
using Botler.Dialogs.Scenari;
using static Botler.Dialogs.Utility.Scenari;
using Botler.Controller;
using Botler.Models;
using MongoDB.Bson;
using Botler.Middleware.Services;
using Botler.Builders;

namespace Botler
{
    public class BotlerAccessors
    {
        public BotlerAccessors(UserState userState, ConversationState conversationState)
        {
            UserState = userState ?? throw new ArgumentNullException(nameof(userState));
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));

            DialogStateAccessor = ConversationState.CreateProperty<DialogState>(nameof(DialogState));
            ScenarioStateAccessors = UserState.CreateProperty<string>(nameof(IScenario));
            PrenotazioneStateAccessor = UserState.CreateProperty<PrenotazioneModel>(nameof(PrenotazioneModel));
            CancellaPrenotazioneStateAccessor = UserState.CreateProperty<PrenotazioneModel>(nameof(PrenotazioneModel));
            VisualizzaTempoStateAccessor = UserState.CreateProperty<PrenotazioneModel>(nameof(PrenotazioneModel));
            VisualizzaPrenotazioneStateAccessor = UserState.CreateProperty<PrenotazioneModel>(nameof(PrenotazioneModel));
            AutenticazioneDipedenteAccessors = UserState.CreateProperty<Dictionary<string, TokenResponse>>("AutenticazioneDipedente");
            QnaActiveAccessors  = UserState.CreateProperty<string>("QnAActive");
            UserModelAccessors = UserState.CreateProperty<List<UserModel>>(nameof(UserModel));
            LastBotStateContextConversation = ConversationState.CreateProperty<Dictionary<string, BotStateContext>>(nameof(BotStateContext));
            TurnCounterConversationAccessors = ConversationState.CreateProperty<Dictionary<string, int>>("TurnCounter");
            LastUserScenarioAccessors = conversationState.CreateProperty<Dictionary<string, IScenario>> ("LastUserScenario");
            TicketAccessors = conversationState.CreateProperty<Ticket>(nameof(Ticket));
            MongoDB = new MongoDBService();
        }

        public UserState UserState { get; set; }

        public MongoDBService MongoDB { get; set; }

        public ConversationState ConversationState { get; set; }
        public IScenario ActiveScenario { get; set; }

        public IStatePropertyAccessor<Ticket> TicketAccessors { get; set; }

        public IStatePropertyAccessor<PrenotazioneModel> PrenotazioneStateAccessor { get; set; }

        public IStatePropertyAccessor<PrenotazioneModel> CancellaPrenotazioneStateAccessor { get; set; }

        public IStatePropertyAccessor<PrenotazioneModel> VisualizzaTempoStateAccessor { get; set; }

        public IStatePropertyAccessor<PrenotazioneModel> VisualizzaPrenotazioneStateAccessor { get; set; }
  
        public IStatePropertyAccessor<DialogState> DialogStateAccessor { get; set; }

        public IStatePropertyAccessor<string> ScenarioStateAccessors { get; set; }
        public IStatePropertyAccessor<Dictionary<string, TokenResponse>> AutenticazioneDipedenteAccessors { get; set; }

        public IStatePropertyAccessor<List<UserModel>> UserModelAccessors { get; set; }

        public IStatePropertyAccessor<string> QnaActiveAccessors { get; set; }

        public IStatePropertyAccessor<Dictionary<string,BotStateContext>> LastBotStateContextConversation { get; set; }

        public IStatePropertyAccessor<Dictionary<string, IScenario>> LastUserScenarioAccessors { get; set; }

        public IStatePropertyAccessor<Dictionary<string,int>> TurnCounterConversationAccessors { get; set; }

        public IStatePropertyAccessor<IList<BsonDocument>> LastBotStateAccessors { get; set; }

        public async Task AddUserToAccessorsListAync(UserModel user, ITurnContext turn)
        {
            List<UserModel> list = await UserModelAccessors.GetAsync(turn, () => new List<UserModel>());
            list.Add(user);
            await UserModelAccessors.SetAsync(turn, list);
        }

        public async Task<UserModel> GetAuthenticatedMemberAsync(ITurnContext turn)
        {
            IList<UserModel> list = await UserModelAccessors.GetAsync(turn, () => new List<UserModel>());
            string memberID = turn.Activity.From.Id;

            foreach(UserModel user in list)
            {
                if(user.Id_Utente.Equals(memberID))
                {
                    return user;
                }
            }
            return null;
        }

        public async Task<int> GetCurretTurnCounterAsync(ITurnContext turn)
        {
           var map = await TurnCounterConversationAccessors.GetAsync(turn, () => new Dictionary<string, int>());
           int countTurn;
           map.TryGetValue(turn.Activity.From.Id, out countTurn);
           return countTurn;
        }

        public async Task SetCurrentScenarioAsync(ITurnContext turn, string scenario)
        {
            await ScenarioStateAccessors.SetAsync(turn, scenario);
            var scenarioActive = ScenarioFactory.FactoryMethod(this, turn, scenario, null);
        }

        public async Task TurnOffQnAAsync(ITurnContext turn)
        {
            await QnaActiveAccessors.SetAsync(turn, Default);
        }

        public async Task ResetScenarioAsync(ITurnContext turn)
        {
            await QnaActiveAccessors.SetAsync(turn, Default);
            await ScenarioStateAccessors.SetAsync(turn, Default);
        }

        public async Task SaveStateAsync(ITurnContext turn)
        {
            await SaveConvStateAsync(turn);
            await SaveUserStateAsyn(turn);
        }

        public async Task SaveConvStateAsync(ITurnContext turnContext)
        {
            await ConversationState.SaveChangesAsync(turnContext);
        }

        public async Task SaveUserStateAsyn(ITurnContext turnContext)
        {
            await UserState.SaveChangesAsync(turnContext);
        }

        public async Task SaveLastBotStateContext(ITurnContext turn, BotStateContext state)
        {
            var map = await LastBotStateContextConversation.GetAsync(turn, () => new Dictionary<string, BotStateContext>());
            var convID = turn.Activity.From.Id;

            if(map.TryGetValue(convID, out state))
            {
                map.Remove(convID);
            }
            map.Add(convID, state);

            await LastBotStateContextConversation.SetAsync(turn, map);
            await SaveStateAsync(turn);

        }

        public async Task UpdateTurnCounterAsync(ITurnContext turn)
        {
            var convID = turn.Activity.From.Id;
            var map = await TurnCounterConversationAccessors.GetAsync(turn, () => new Dictionary<string,int>());
            var turnValue = 0;

            if(map.TryGetValue(convID, out turnValue))
            {
                map.Remove(convID);
            }

            map.Add(convID, ++turnValue);

            await TurnCounterConversationAccessors.SetAsync(turn, map);
            await SaveStateAsync(turn);
        }

        public async Task AddAuthUserAsync(ITurnContext turn, TokenResponse token)
        {
            var dic = await AutenticazioneDipedenteAccessors.GetAsync(turn, () => new Dictionary<string, TokenResponse>());
            dic.Add(turn.Activity.From.Id, token);
            await AutenticazioneDipedenteAccessors.SetAsync(turn, dic);
            await SaveStateAsync(turn);
        }

        public async Task<TokenResponse> GetUserToken(ITurnContext turn)
        {
            var dic = await AutenticazioneDipedenteAccessors.GetAsync(turn, () => new Dictionary<string, TokenResponse>());
            TokenResponse token;
            var convID = turn.Activity.From.Id;
            dic.TryGetValue(convID, out token);
            await SaveStateAsync(turn);
            return token;
        }

        public async Task<BotStateContext> GetLastBotStateContextCByConvIDAsync(ITurnContext turn)
        {
            var convID = turn.Activity.From.Id;
            IList<BotStateContext> list = await MongoDB.GetAllBotStateByConvIDAsync(convID);

            if(list.Count > 0)
            {
                return list[list.Count - 1];
            }
            else
            {
                return new BotStateContext();
            }

        }

        public async Task SaveLastUserScenarioAsync(ITurnContext turn, IScenario scenario)
        {
            var dic = await LastUserScenarioAccessors.GetAsync(turn, () => new Dictionary<string, IScenario>());
            var convID = turn.Activity.From.Id;
            dic.Add(convID, scenario);
            await LastUserScenarioAccessors.SetAsync(turn, dic);
            await SaveStateAsync(turn);
        }

        public async Task<IScenario> GetLastUserScenarioAsync(ITurnContext turn)
        {
            var dic = await LastUserScenarioAccessors.GetAsync(turn, () => new Dictionary<string, IScenario>());
            var convID = turn.Activity.From.Id;
            IScenario scenario;
            dic.TryGetValue(convID, out scenario);
            await SaveStateAsync(turn);
            return scenario;
        }
    }
}