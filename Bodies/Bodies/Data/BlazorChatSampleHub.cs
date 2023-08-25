using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System;
using MudBlazor;
using System.Net.NetworkInformation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection.XmlEncryption;

namespace BlazorChat
{
    public class BlazorChatSampleHub : Hub
    {
        public const string HubUrl = "/chat";

        private static Random _random = new Random();

        public async Task Broadcast(string username, string message, string groupName)
        {
            if(message.Contains("has left the lobby."))
            {
                UserHandler.ConnectedUsers.Remove(Context.ConnectionId);
                await Clients.Group(groupName).SendAsync("GetPlayerCount", UserHandler.GetCountByGroup(groupName));
            }
            await Clients.Group(groupName).SendAsync("Broadcast", username, message);
        }

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception e)
        {
            await base.OnDisconnectedAsync(e);
        }

        public async Task AddGroup(string groupName)
        {
            UserHandler.ConnectedUsers.Add(Context.ConnectionId, new User () { GroupName = groupName });

            if(UserHandler.GetCountByGroup(groupName) == 1)
            {
                //Must be starting a new group - assign user to be admin
                await Clients.Client(Context.ConnectionId).SendAsync("AssignAdmin");
            }
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("GetPlayerCount", UserHandler.GetCountByGroup(groupName));
        }

        public async Task StartGame(string groupName, int numberOfMafia)
        {
            if (UserHandler.GetCountByGroup(groupName) / numberOfMafia > 2)
            {
                for (int i = 0; i < numberOfMafia; i++)
                {
                    var nonMafias = UserHandler.GetNonMafiaUser(groupName);
                    var temp = nonMafias.ElementAt(_random.Next(nonMafias.Count()));
                    temp.Value.IsMafia = true;
                    if(UserHandler.ConnectedUsers.TryGetValue(temp.Key, out var user))
                    {
                        UserHandler.ConnectedUsers[temp.Key] = user;
                    }
                }

                foreach(var x in UserHandler.GetUsersByGroup(groupName))
                {
                    await Clients.Client(x.Key).SendAsync("GameStarted", x.Value.GameStartedMessage);
                }
            } 
            else
            {
                await Clients.Group(groupName).SendAsync("Broadcast","System","Not enough players to start game, get more players or have less mafia");
            }
        }
    }

    public static class UserHandler
    {
        public static Dictionary<string, User> ConnectedUsers = new Dictionary<string, User>();

        public static int GetCountByGroup(string groupName)
        {
            Dictionary<string, User> temp = new Dictionary<string, User>();

            foreach (var x in ConnectedUsers)
            {
                if (x.Value.GroupName == groupName)
                {
                    temp.Add(x.Key, x.Value);
                }
            }
            return temp.Count();
        }

        public static Dictionary<string, User> GetUsersByGroup(string groupName)
        {
            Dictionary<string, User> temp = new Dictionary<string, User>();

            foreach (var x in ConnectedUsers)
            {
                if(x.Value.GroupName == groupName)
                {
                    temp.Add(x.Key, x.Value);
                }
            }
            return temp;
        }

        public static Dictionary<string, User> GetNonMafiaUser(string groupName)
        {
            Dictionary<string, User> temp = new Dictionary<string, User>();
            var group = GetUsersByGroup(groupName);

            foreach (var x in group)
            {
                if (!x.Value.IsMafia)
                {
                    temp.Add(x.Key, x.Value);
                }
            }
            return temp;
        }
    }

    public class User
    {
        public string UserName { get; set; } = string.Empty;

        public string GroupName { get; set; } = string.Empty;

        public bool IsMafia = false;
        public string GameStartedMessage => IsMafia ? "<FONT COLOR='#ff0000'>YOU ARE THE MAFIA!!</FONT>" : "YOU ARE NOT THE MAFIA!";
    }
}