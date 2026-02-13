using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AdessoWorldLeague.Business.Interfaces;
using AdessoWorldLeague.Dto.Draw;

namespace AdessoWorldLeague.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DrawController : ControllerBase
{
    private readonly IDrawService _drawService;

    public DrawController(IDrawService drawService)
    {
        _drawService = drawService;
    }

    [HttpPost("performDraw")]
    public async Task<IActionResult> PerformDraw([FromBody] DrawRequest request)
    {
        var result = await _drawService.PerformDrawAsync(request);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDraw(string id)
    {
        var result = await _drawService.GetDrawByIdAsync(id);
        if (!result.IsSuccess)
            return NotFound(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    [HttpGet("getAllDraws")]
    public async Task<IActionResult> GetAllDraws()
    {
        var result = await _drawService.GetAllDrawsAsync();
        return Ok(result.Data);
    }
}
