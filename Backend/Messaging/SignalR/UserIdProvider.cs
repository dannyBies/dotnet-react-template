using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Example.Messaging.SignalR
{
    public class UserIdProvider : IUserIdProvider
    {
        private readonly ILogger<UserIdProvider> _logger;

        public UserIdProvider(ILogger<UserIdProvider> logger)
        {
            _logger = logger;
        }

        public virtual string GetUserId(HubConnectionContext connection)
        {
            var userId = connection.User?.FindFirstValue("userId");
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogError($"Unable to find the userId claim for '{connection.User?.Identity?.Name}'");
                return "<unknown-user-id>";
            }

            return userId;
        }
    }
}
