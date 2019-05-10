using Microsoft.Bot.Schema;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Attachment = Microsoft.Bot.Schema.Attachment;
using System.Collections.Generic;

namespace Botler.Controller
{
    public abstract class SendHeroCard : ISendAttachment
    {
        public async Task SendAttachmentAsync(ITurnContext turn)
        {
           Attachment attachment = CreateAttachement();
           var response = CreateResponse(turn, attachment);
           await SendHeroCardAsync(turn, response);
        }

        public abstract Attachment CreateAttachement();

        public  Activity CreateResponse(ITurnContext turn, Attachment attachment)
        {
            var activity = turn.Activity;
            var response = activity.CreateReply();
            response.Attachments = new List<Attachment>() { attachment };
            return response;
        }

        public async Task SendHeroCardAsync(ITurnContext turn, Activity response)
        {
            await turn.SendActivityAsync(response).ConfigureAwait(false);
        }
    }
}