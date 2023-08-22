using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Example.Messaging.MassTransit.Contracts;
using Example.Messaging.SignalR;

namespace Example.MassTransit.Messaging.Consumers
{
    public class UserCreatedConsumer :
    IConsumer<UserCreated>
    {
        private readonly ILogger<UserCreatedConsumer> _logger;
        private readonly IHubContext<ExampleHub> _hubContext;

        public UserCreatedConsumer(ILogger<UserCreatedConsumer> logger, IHubContext<ExampleHub> hubContext)
        {
            _logger = logger;
            _hubContext = hubContext;
        }
        public async Task Consume(ConsumeContext<UserCreated> context)
        {
            await _hubContext.Clients.All.SendAsync("NewMessage", $"A new user has been created: '{context.Message.UserId}'");
        }
    }
}
