using MassTransit;
using System.Security.Claims;

namespace Example.Messaging.MassTransit
{
    internal class HttpPublishObserver : IPublishObserver
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public HttpPublishObserver(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public Task PostPublish<T>(PublishContext<T> context) where T : class
        {
            return Task.CompletedTask;
        }


        public Task PrePublish<T>(PublishContext<T> context) where T : class
        {
            var userId = httpContextAccessor.HttpContext?.User.FindFirstValue("userId");
            if (userId != null)
            {
                context.Headers.Set("InitiatedByUser", userId);
            }
            return Task.CompletedTask;
        }


        public Task PublishFault<T>(PublishContext<T> context, Exception exception) where T : class
        {
            return Task.CompletedTask;
        }
    }
}