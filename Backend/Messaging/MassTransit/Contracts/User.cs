namespace Example.Messaging.MassTransit.Contracts;

public record CreateUser(string ExternalId, string ConnectionName, string? Email);
public record UserCreated(Guid UserId);
