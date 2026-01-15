using MediatR;

namespace Application.Features.Auth.RegisterUser;

public record RegisterUserCommand(string Username, string Password, string RoleCode) : IRequest<RegisterUserResponse>;
