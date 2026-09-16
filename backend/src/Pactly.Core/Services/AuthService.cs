using MongoDB.Bson;
using Pactly.Core.Domain;
using Pactly.Core.Repositories;

namespace Pactly.Core.Services;

public class EmailAlreadyRegisteredException : Exception
{
    public EmailAlreadyRegisteredException(string email)
        : base($"An account with email '{email}' already exists.")
    {
    }
}

public class AuthService
{
    private readonly IUserRepository _userRepository;

    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User> RegisterAsync(string email, string password, string displayName)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var existing = await _userRepository.GetByEmailAsync(normalizedEmail);
        if (existing is not null)
        {
            throw new EmailAlreadyRegisteredException(normalizedEmail);
        }

        var user = new User
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            DisplayName = displayName.Trim(),
            CreatedAt = DateTimeOffset.UtcNow,
            CurrentSeats = 0
        };

        await _userRepository.CreateAsync(user);
        return user;
    }

    public async Task<User?> ValidateCredentialsAsync(string email, string password)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(normalizedEmail);
        if (user is null)
        {
            return null;
        }

        return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash) ? user : null;
    }
}
