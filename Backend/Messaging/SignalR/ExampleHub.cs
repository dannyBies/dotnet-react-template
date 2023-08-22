using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Example.Messaging.SignalR
{
    [Authorize]
    public class ExampleHub : Hub
    {
        public async override Task OnConnectedAsync()
        {
            await Clients.All.SendAsync("NewMessage", "New user connected, user id = " + Context.UserIdentifier);
            await base.OnConnectedAsync();
        }
    }
}
