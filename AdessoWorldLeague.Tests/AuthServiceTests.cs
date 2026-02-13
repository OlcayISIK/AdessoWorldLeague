using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Moq;
using AdessoWorldLeague.Business;
using AdessoWorldLeague.Business.Services;
using AdessoWorldLeague.Business.Settings;
using AdessoWorldLeague.Data.Documents;
using AdessoWorldLeague.Dto.Auth;
using AdessoWorldLeague.Repository.Interfaces;

namespace AdessoWorldLeague.Tests;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepository;
    private readonly Mock<IStringLocalizer<Messages>> _localizer;
    private readonly AuthService _sut;

    private static readonly JwtSettings TestJwtSettings = new()
    {
        Secret = "k3N6dVh3WjRrU2pNcDluRjZ0QmEyY0Q3ZkhyVjltU1l4V1VwTg==",
        Issuer = "TestIssuer",
        Audience = "TestAudience",
        ExpirationMinutes = 60
    };

    public AuthServiceTests()
    {
        _userRepository = new Mock<IUserRepository>();
        _localizer = new Mock<IStringLocalizer<Messages>>();

        _localizer.Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));

        var jwtOptions = Options.Create(TestJwtSettings);
        _sut = new AuthService(_userRepository.Object, jwtOptions, _localizer.Object);
    }

    // --- Register Tests ---

    [Fact]
    public async Task RegisterAsync_NewEmail_ReturnsSuccess()
    {
        _userRepository.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((UserDocument?)null);
        _userRepository.Setup(r => r.CreateAsync(It.IsAny<UserDocument>()))
            .Returns(Task.CompletedTask);

        var request = new RegisterRequest
        {
            Email = "test@adesso.com",
            Password = "password123",
            FirstName = "John",
            LastName = "Doe"
        };

        var result = await _sut.RegisterAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal("RegistrationSuccessful", result.Message);
    }

    [Fact]
    public async Task RegisterAsync_ExistingEmail_ReturnsFailure()
    {
        _userRepository.Setup(r => r.GetByEmailAsync("existing@adesso.com"))
            .ReturnsAsync(new UserDocument
            {
                Email = "existing@adesso.com",
                PasswordHash = "hash",
                FirstName = "Jane",
                LastName = "Doe"
            });

        var request = new RegisterRequest
        {
            Email = "existing@adesso.com",
            Password = "password123",
            FirstName = "John",
            LastName = "Doe"
        };

        var result = await _sut.RegisterAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("EmailAlreadyRegistered", result.Message);
    }

    [Fact]
    public async Task RegisterAsync_NewUser_HashesPassword()
    {
        UserDocument? savedUser = null;
        _userRepository.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((UserDocument?)null);
        _userRepository.Setup(r => r.CreateAsync(It.IsAny<UserDocument>()))
            .Callback<UserDocument>(u => savedUser = u)
            .Returns(Task.CompletedTask);

        var request = new RegisterRequest
        {
            Email = "test@adesso.com",
            Password = "password123",
            FirstName = "John",
            LastName = "Doe"
        };

        await _sut.RegisterAsync(request);

        Assert.NotNull(savedUser);
        Assert.NotEqual("password123", savedUser.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("password123", savedUser.PasswordHash));
    }

    // --- Login Tests ---

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsToken()
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("password123");
        _userRepository.Setup(r => r.GetByEmailAsync("test@adesso.com"))
            .ReturnsAsync(new UserDocument
            {
                Id = "507f1f77bcf86cd799439011",
                Email = "test@adesso.com",
                PasswordHash = hashedPassword,
                FirstName = "John",
                LastName = "Doe",
                Role = "User"
            });

        var request = new LoginRequest
        {
            Email = "test@adesso.com",
            Password = "password123"
        };

        var result = await _sut.LoginAsync(request);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.False(string.IsNullOrEmpty(result.Data.Token));
        Assert.True(result.Data.Expiration > DateTime.UtcNow);
    }

    [Fact]
    public async Task LoginAsync_NonExistentEmail_ReturnsFailure()
    {
        _userRepository.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((UserDocument?)null);

        var request = new LoginRequest
        {
            Email = "unknown@adesso.com",
            Password = "password123"
        };

        var result = await _sut.LoginAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("InvalidCredentials", result.Message);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ReturnsFailure()
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("correctPassword");
        _userRepository.Setup(r => r.GetByEmailAsync("test@adesso.com"))
            .ReturnsAsync(new UserDocument
            {
                Id = "507f1f77bcf86cd799439011",
                Email = "test@adesso.com",
                PasswordHash = hashedPassword,
                FirstName = "John",
                LastName = "Doe",
                Role = "User"
            });

        var request = new LoginRequest
        {
            Email = "test@adesso.com",
            Password = "wrongPassword"
        };

        var result = await _sut.LoginAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("InvalidCredentials", result.Message);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_TokenContainsCorrectClaims()
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("password123");
        _userRepository.Setup(r => r.GetByEmailAsync("test@adesso.com"))
            .ReturnsAsync(new UserDocument
            {
                Id = "507f1f77bcf86cd799439011",
                Email = "test@adesso.com",
                PasswordHash = hashedPassword,
                FirstName = "John",
                LastName = "Doe",
                Role = "Admin"
            });

        var request = new LoginRequest
        {
            Email = "test@adesso.com",
            Password = "password123"
        };

        var result = await _sut.LoginAsync(request);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(result.Data!.Token);

        Assert.Equal("test@adesso.com", jwt.Claims.First(c => c.Type == ClaimTypes.Email).Value);
        Assert.Equal("John", jwt.Claims.First(c => c.Type == ClaimTypes.GivenName).Value);
        Assert.Equal("Doe", jwt.Claims.First(c => c.Type == ClaimTypes.Surname).Value);
        Assert.Equal("Admin", jwt.Claims.First(c => c.Type == ClaimTypes.Role).Value);
        Assert.Equal("TestIssuer", jwt.Issuer);
        Assert.Contains("TestAudience", jwt.Audiences);
    }
}
