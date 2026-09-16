using Pactly.Core.Services;
using Pactly.Tests.Fakes;
using Xunit;

namespace Pactly.Tests.Services;

public class AuthServiceTests
{
    [Fact]
    public async Task RegisterAsync_CreatesUser_WithHashedPassword()
    {
        var service = new AuthService(new InMemoryUserRepository());

        var user = await service.RegisterAsync("Jane@Example.com", "correct-horse", "Jane Doe");

        Assert.Equal("jane@example.com", user.Email);
        Assert.NotEqual("correct-horse", user.PasswordHash);
        Assert.NotEmpty(user.Id);
    }

    [Fact]
    public async Task RegisterAsync_ThrowsWhenEmailAlreadyExists()
    {
        var repository = new InMemoryUserRepository();
        var service = new AuthService(repository);
        await service.RegisterAsync("jane@example.com", "correct-horse", "Jane Doe");

        await Assert.ThrowsAsync<EmailAlreadyRegisteredException>(() =>
            service.RegisterAsync("jane@example.com", "another-password", "Jane Doe Again"));
    }

    [Fact]
    public async Task ValidateCredentialsAsync_ReturnsUser_WhenPasswordMatches()
    {
        var repository = new InMemoryUserRepository();
        var service = new AuthService(repository);
        await service.RegisterAsync("jane@example.com", "correct-horse", "Jane Doe");

        var result = await service.ValidateCredentialsAsync("jane@example.com", "correct-horse");

        Assert.NotNull(result);
        Assert.Equal("jane@example.com", result!.Email);
    }

    [Fact]
    public async Task ValidateCredentialsAsync_ReturnsNull_WhenPasswordIsWrong()
    {
        var repository = new InMemoryUserRepository();
        var service = new AuthService(repository);
        await service.RegisterAsync("jane@example.com", "correct-horse", "Jane Doe");

        var result = await service.ValidateCredentialsAsync("jane@example.com", "wrong-password");

        Assert.Null(result);
    }

    [Fact]
    public async Task ValidateCredentialsAsync_ReturnsNull_WhenUserDoesNotExist()
    {
        var service = new AuthService(new InMemoryUserRepository());

        var result = await service.ValidateCredentialsAsync("nobody@example.com", "whatever");

        Assert.Null(result);
    }
}
