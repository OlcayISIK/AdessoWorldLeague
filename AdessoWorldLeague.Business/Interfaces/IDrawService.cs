using AdessoWorldLeague.Core.Results;
using AdessoWorldLeague.Dto.Draw;

namespace AdessoWorldLeague.Business.Interfaces;

public interface IDrawService
{
    Task<OperationResult<DrawResponse>> PerformDrawAsync(DrawRequest request);
    Task<OperationResult<DrawResponse>> GetDrawByIdAsync(string id);
    Task<OperationResult<List<DrawResponse>>> GetAllDrawsAsync();
}
