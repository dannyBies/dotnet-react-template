namespace Example.Database.Entities
{
    public class User : Entity
    {
        public required string ExternalId { get; set; }
        public required string ConnectionName { get; set; }
        public string? Email { get; set; }
    }
}
