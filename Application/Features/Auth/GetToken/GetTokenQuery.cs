using MediatR;

namespace Application.Features.Auth.GetToken;

public record GetTokenQuery(string Username, string Password) : IRequest<GetTokenResponse?>;
