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
            AutenticazioneDipedenteAccessors = UserState.CreateProperty<Dictionary<string,bool>>("AutenticazioneDipedente");
            QnaActiveAccessors  = UserState.CreateProperty<string>("QnAActive");
            UserModelAccessors = UserState.CreateProperty<List<UserModel>>(nameof(UserModel));

        }

        public UserState UserState { get; set; }

        public ConversationState ConversationState { get; set; }

        public IScenario ActiveScenario { get; set; }

        public IStatePropertyAccessor<PrenotazioneModel> PrenotazioneStateAccessor { get; set; }

        public IStatePropertyAccessor<PrenotazioneModel> CancellaPrenotazioneStateAccessor { get; set; }

        public IStatePropertyAccessor<PrenotazioneModel> VisualizzaTempoStateAccessor { get; set; }

        public IStatePropertyAccessor<PrenotazioneModel> VisualizzaPrenotazioneStateAccessor { get; set; }

        public IStatePropertyAccessor<DialogState> DialogStateAccessor { get; set; }

        public IStatePropertyAccessor<string> ScenarioStateAccessors { get; set; }

        public IStatePropertyAccessor<Dictionary<string,bool>> AutenticazioneDipedenteAccessors { get; set; }

        public IStatePropertyAccessor<List<UserModel>> UserModelAccessors { get; set; }

        public IStatePropertyAccessor<string> AuthTimeAccessors { get; set; }

        public IStatePropertyAccessor<string> QnaActiveAccessors { get; set; }

        public IStatePropertyAccessor<string> ResourceFileSelectedAccessors { get; set; }

        public async Task AddUserToAccessorsListAync(UserModel user, ITurnContext turn)
        {
            List<UserModel> list = await UserModelAccessors.GetAsync(turn, () => new List<UserModel>());
            list.Add(user);
            await UserModelAccessors.SetAsync(turn, list);
        }

        public async Task AddAuthenticatedUserAsync( ITurnContext turn)
        {
            string memberID = turn.Activity.From.Id;
            Dictionary<string,bool> map = await AutenticazioneDipedenteAccessors.GetAsync(turn, () => new Dictionary<string,bool>());
            map.Add(memberID, true);
            await AutenticazioneDipedenteAccessors.SetAsync(turn, map);
        }

        public async Task<UserModel> GetAuthenticatedMemberAsyc(ITurnContext turn)
        {
            List<UserModel> list = await UserModelAccessors.GetAsync(turn, () => new List<UserModel>());
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

        public async Task SetCurrentScenarioAsync(ITurnContext turn, string scenario)
        {
            await ScenarioStateAccessors.SetAsync(turn, scenario);
            ActiveScenario = ScenarioFactory.FactoryMethod(this, turn, scenario);
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

        public async Task SaveStateAsync(ITurnContext currentTurn)
        {
            await SaveConvStateAsync(currentTurn);
            await SaveUserStateAsyn(currentTurn);
        }

        public async Task SaveConvStateAsync(ITurnContext turnContext)
        {
               await ConversationState.SaveChangesAsync(turnContext);
        }

        public async Task SaveUserStateAsyn(ITurnContext turnContext)
        {
            await UserState.SaveChangesAsync(turnContext);
        }

    }
}