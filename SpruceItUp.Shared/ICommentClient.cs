using System.Threading.Tasks;
using SpruceItUp.Shared.Models;
namespace SpruceItUp.Shared
{
    public interface ICommentClient
    {
        Task ReceiveComment(Comment comment);
    }
}