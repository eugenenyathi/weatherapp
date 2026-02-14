using Microsoft.AspNetCore.Mvc;
using weatherapp.Services.Interfaces;
using weatherapp.DataTransferObjects;
using weatherapp.Requests;

namespace weatherapp.Controllers;

[ApiController]
[Route("api/preferences")]
public class UserPreferenceController(IUserPreferenceService userPreferenceService) : ControllerBase
{
    [HttpGet("{userId}")]
    public async Task<ActionResult<UserPreferenceDto?>> GetUserPreference(Guid userId)
    {
        return Ok(await userPreferenceService.GetByUserIdAsync(userId));
    }

    [HttpPost]
    public async Task<ActionResult<UserPreferenceDto>> CreateUserPreference(
        [FromQuery] Guid userId, 
        [FromBody] UserPreferenceRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
            
        return Ok(await userPreferenceService.CreateAsync(userId, request));
    }

    [HttpPut("{preferenceId}")]
    public async Task<ActionResult<UserPreferenceDto>> UpdateUserPreference(
        Guid preferenceId, 
        [FromBody] UserPreferenceRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
            
        return Ok(await userPreferenceService.UpdateAsync(preferenceId, request));
    }

    [HttpDelete("{preferenceId}")]
    public async Task<ActionResult> DeleteUserPreference(Guid preferenceId)
    {
        await userPreferenceService.DeleteAsync(preferenceId);
        return NoContent();
    }
}