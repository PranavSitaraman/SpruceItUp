using System.Threading.Tasks;
using SpruceItUp.Shared.Models;
namespace SpruceItUp.Shared
{
    public interface IPinClient
    {
        Task ReceivePin(Pin pin);
    }
}