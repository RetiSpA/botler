using System.Threading.Tasks;

namespace Botler.Model
{
    public interface  ICommand
    {
        Task ExecuteCommandAsync();
    }
}