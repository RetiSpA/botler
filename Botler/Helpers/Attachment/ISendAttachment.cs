using System.Threading.Tasks;
using Microsoft.Bot.Builder;

namespace Botler.Controller
{
    public interface ISendAttachment
    {
        Task SendAttachmentAsync(ITurnContext turn);
    }
}