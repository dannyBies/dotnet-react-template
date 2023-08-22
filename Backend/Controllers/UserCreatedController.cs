using Example.Messaging.MassTransit.Contracts;
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace Example.Controllers
{
    public record CreateUserRequest(string ExternalId, string ConnectionName, string? Email);
    public record CreateUserSucceededResponse(Guid UserId);

    public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
    {
        public CreateUserRequestValidator()
        {
            RuleFor(user => user.ExternalId).NotEmpty();
            RuleFor(user => user.ConnectionName).NotEmpty();
            RuleFor(user => user.Email).EmailAddress().Unless(x => string.IsNullOrEmpty(x.Email));
        }
    }

    [ApiController]
    [Route("api/users")]
    public class UserCreatedController : BaseController
    {
        private readonly IValidator<CreateUserRequest> _validator;
        private readonly IRequestClient<CreateUser> _client;

        public UserCreatedController(ILogger<UserCreatedController> logger, IValidator<CreateUserRequest> validator, IRequestClient<CreateUser> client) : base(logger)
        {
            _validator = validator;
            _client = client;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            if (!IsValidRequest(_validator, request, out var validationErrors))
            {
                return ValidationProblem(validationErrors);
            }

            var response = await _client.GetResponse<CreateUserSucceededResponse>(new CreateUser(request.ExternalId, request.ConnectionName, request.Email));
            return Ok(response.Message);
        }
    }
}
