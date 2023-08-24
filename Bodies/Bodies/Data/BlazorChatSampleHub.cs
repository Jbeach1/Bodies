using Microsoft.AspNetCore.SignalR;

namespace BlazorChat
{
    public class BlazorChatSampleHub : Hub
    {
        public const string HubUrl = "/chat";

        
        public async Task Broadcast(string username, string message, string groupName)
        {
            await Clients.Group(groupName).SendAsync("Broadcast", username, message);
        }
        
        /*
        public async Task Broadcast(string username, string message)
        {
            await Clients.All.SendAsync("Broadcast", username, message);
        }
        */

        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"{Context.ConnectionId} connected");
            return base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception e)
        {
            Console.WriteLine($"Disconnected {e?.Message} {Context.ConnectionId}");
            await base.OnDisconnectedAsync(e);
        }

        public async Task AddGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }
    }
}