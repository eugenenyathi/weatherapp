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
    public async Task<ActionResult<UserPreferenceDto>> Get(Guid userId)
    {
        return Ok(await userPreferenceService.GetByUserIdAsync(userId));
    }

    [HttpPost("{userId}")]
    public async Task<ActionResult<UserPreferenceDto>> Create(
         Guid userId,
        [FromBody] UserPreferenceRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        return Ok(await userPreferenceService.CreateAsync(userId, request));
    }

    [HttpPut("{userId}/{preferenceId}")]
    public async Task<ActionResult<UserPreferenceDto>> Update(
        Guid userId,
        Guid preferenceId,
        [FromBody] UserPreferenceRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        return Ok(await userPreferenceService.UpdateAsync(userId, preferenceId, request));
    }

    [HttpDelete("{userId}/{preferenceId}")]
    public async Task<ActionResult> Delete(Guid userId, Guid preferenceId)
    {
        await userPreferenceService.DeleteAsync(userId, preferenceId);
        return NoContent();
    }
}