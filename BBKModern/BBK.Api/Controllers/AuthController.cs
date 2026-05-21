using BBK.Api.Models;
using BBK.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BBK.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IBbkService service) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<ApiResult<LoginResponse>>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await service.LoginAsync(request, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
