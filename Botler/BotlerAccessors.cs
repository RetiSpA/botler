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

namespace Botler
{
    public class BotlerAccessors
    {
        public BotlerAccessors(UserState userState, ConversationState conversationState)
        {
            UserState = userState ?? throw new ArgumentNullException(nameof(userState));
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));

            DialogStateAccessor = ConversationState.CreateProperty<DialogState>(nameof(DialogState));
            PrenotazioneStateAccessor = UserState.CreateProperty<PrenotazioneModel>(nameof(PrenotazioneModel));
            CancellaPrenotazioneStateAccessor = UserState.CreateProperty<PrenotazioneModel>(nameof(PrenotazioneModel));
            VisualizzaTempoStateAccessor = UserState.CreateProperty<PrenotazioneModel>(nameof(PrenotazioneModel));
            VisualizzaPrenotazioneStateAccessor = UserState.CreateProperty<PrenotazioneModel>(nameof(PrenotazioneModel));
            ScenarioStateAccessors = ConversationState.CreateProperty<IScenario>(nameof(IScenario));
        }

        public UserState UserState { get; set; }

        public ConversationState ConversationState { get; set; }

        public IStatePropertyAccessor<PrenotazioneModel> PrenotazioneStateAccessor { get; set; }

        public IStatePropertyAccessor<PrenotazioneModel> CancellaPrenotazioneStateAccessor { get; set; }

        public IStatePropertyAccessor<PrenotazioneModel> VisualizzaTempoStateAccessor { get; set; }

        public IStatePropertyAccessor<PrenotazioneModel> VisualizzaPrenotazioneStateAccessor { get; set; }

        public IStatePropertyAccessor<DialogState> DialogStateAccessor { get; set; }

        public IStatePropertyAccessor<IScenario> ScenarioStateAccessors { get; set; }

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