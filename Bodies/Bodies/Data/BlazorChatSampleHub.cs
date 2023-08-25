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

        public override Task OnConnectedAsync()
        {
            UserHandler.ConnectedIds.Add(Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception e)
        {
            UserHandler.ConnectedIds.Remove(Context.ConnectionId);
            await base.OnDisconnectedAsync(e);
        }

        public async Task AddGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }
    }

    public static class UserHandler
    {
        public static HashSet<string> ConnectedIds = new HashSet<string>();
    }
}