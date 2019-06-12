using System.Threading.Tasks;

namespace Botler.Commands
{
    public interface ICommand
    {
        Task ExecuteCommandAsync();
    }
}