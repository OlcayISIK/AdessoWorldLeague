using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using AdessoWorldLeague.Business.Interfaces;
using AdessoWorldLeague.Business.Settings;
using AdessoWorldLeague.Core.Results;
using AdessoWorldLeague.Data.Documents;
using AdessoWorldLeague.Dto.Auth;
using AdessoWorldLeague.Repository.Interfaces;

namespace AdessoWorldLeague.Business.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly JwtSettings _jwtSettings;

    public AuthService(IUserRepository userRepository, IOptions<JwtSettings> jwtSettings)
    {
        _userRepository = userRepository;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<OperationResult> RegisterAsync(RegisterRequest request)
    {
        var existing = await _userRepository.GetByEmailAsync(request.Email);
        if (existing is not null)
            return OperationResult.Failure("This email address is already registered.");

        var user = new UserDocument
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = "User"
        };

        await _userRepository.CreateAsync(user);
        return OperationResult.Success();
    }

    public async Task<OperationResult<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user is null)
            return OperationResult<LoginResponse>.Failure("Email or password is incorrect.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return OperationResult<LoginResponse>.Failure("Email or password is incorrect.");

        var token = GenerateToken(user);
        var expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);

        return OperationResult<LoginResponse>.Success(new LoginResponse
        {
            Token = token,
            Expiration = expiration
        });
    }

    private string GenerateToken(UserDocument user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
