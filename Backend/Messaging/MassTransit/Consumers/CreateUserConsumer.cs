using MassTransit;
using Example.Controllers;
using Example.Database;
using Example.Messaging.MassTransit.Contracts;

namespace Example.MassTransit.Messaging.Consumers
{
    public class CreateUserConsumer : IConsumer<CreateUser>
    {
        private readonly ILogger<CreateUserConsumer> _logger;

        public CreateUserConsumer(ILogger<CreateUserConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<CreateUser> context)
        {
            var newUser = new User(context.Message.ExternalId, context.Message.ConnectionName, context.Message.Email);
            using (var dbContext = new ExampleDbContext())
            {
                dbContext.Users.EnsureDoesNotExist(user => user.ExternalId == newUser.ExternalId);
                var user = dbContext.Users.Add(newUser);
                await dbContext.SaveChangesAsync();
                await context.RespondAsync(new CreateUserSucceededResponse(newUser.Id));
                await context.Publish(new UserCreated(newUser.Id));
            }
        }
    }
}
