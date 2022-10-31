using System;
using System.Threading.Tasks;
using SpruceItUp.Shared;
using Microsoft.AspNetCore.SignalR;
namespace SpruceItUp.Backend.Hubs
{
    public class CommentHub: Hub<ICommentClient>
    {
        public async Task JoinPinRoom(Guid pinId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(pinId));
        }
         public static string GetGroupName(Guid pinId)
        {
            return $"PNG|{pinId}";
        }
    }
}