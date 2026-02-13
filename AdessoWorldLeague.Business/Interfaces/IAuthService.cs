using AdessoWorldLeague.Core.Results;
using AdessoWorldLeague.Dto.Auth;

namespace AdessoWorldLeague.Business.Interfaces;

public interface IAuthService
{
    Task<OperationResult> RegisterAsync(RegisterRequest request);
    Task<OperationResult<LoginResponse>> LoginAsync(LoginRequest request);
}
