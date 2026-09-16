using Pactly.Core.Domain;

namespace Pactly.Api.DTOs;

public record AuthPayload(string Token, User User);
