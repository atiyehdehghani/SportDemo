using MediatR;
using SportMarket.Application.Common.Dtos;

namespace SportMarket.Application.Features.Auth;

public record RegisterCommand(string FullName, string Email, string Password) : IRequest<AuthResponseDto>;
