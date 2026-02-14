using Microsoft.AspNetCore.Mvc;
using weatherapp.Services.Interfaces;
using weatherapp.DataTransferObjects;
using weatherapp.Requests;

namespace weatherapp.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid) 
            return BadRequest(ModelState);
            
        return Ok(await authService.Register(request));
    }

    [HttpPut("update/{userId}")]
    public async Task<ActionResult<UserDto>> Update(Guid userId, [FromBody] UpdateRequest request)
    {
        if (!ModelState.IsValid) 
            return BadRequest(ModelState);
            
        return Ok(await authService.Update(userId, request));
    }
}