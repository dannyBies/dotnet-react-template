using MassTransit;

namespace Example.Messaging.MassTransit;

public class LoggingReceiveObserver : IReceiveObserver
{
    private readonly ILogger<LoggingReceiveObserver> _logger;

    public LoggingReceiveObserver(ILogger<LoggingReceiveObserver> logger)
    {
        _logger = logger;
    }

    public Task ConsumeFault<T>(ConsumeContext<T> context, TimeSpan duration, string consumerType, Exception exception) where T : class
    {
        _logger.LogError(exception, "The consumer '{consumerType}' faulted.", consumerType);
        return Task.CompletedTask;
    }

    public Task PostConsume<T>(ConsumeContext<T> context, TimeSpan duration, string consumerType) where T : class
    {
        var message = context.Message.ToString();

        _logger.LogDebug("Message '{message} has been successfully consumed by {consumerType}", message, consumerType);
        return Task.CompletedTask;
    }

    public Task PostReceive(ReceiveContext context)
    {
        return Task.CompletedTask;
    }

    public Task PreReceive(ReceiveContext context)
    {
        return Task.CompletedTask;
    }

    public Task ReceiveFault(ReceiveContext context, Exception exception)
    {
        _logger.LogError(exception, "The transport receive faulted.");
        return Task.CompletedTask;
    }
}
