using System;
using System.Threading.Tasks;
using SpruceItUp.Shared;
using Microsoft.AspNetCore.SignalR;
using static SpruceItUp.Shared.MathExt;
namespace SpruceItUp.Backend.Hubs
{
    public class PinHub : Hub<IPinClient>
    {
        public static Position PosTileSize = new(0.01, 0.01);
        public async Task JoinSurroundingTiles(Position position)
        {
            var centerTile = position.FloorRound();
            foreach ((int, int) n in Neighbors)
            {
                string groupName = GetGroupName(centerTile + PosTileSize * n);
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            }
        }
        public static string GetGroupName(Position position)
        {
            return $"SRG|{Guid.Empty.ToString()}|{position.Lat}|{position.Lon}";
        }
    }
}