using Microsoft.AspNetCore.Mvc;
using weatherapp.Services.Interfaces;
using weatherapp.DataTransferObjects;
using weatherapp.Requests;

namespace weatherapp.Controllers;

[ApiController]
[Route("api/track")]
public class TrackLocationController(ITrackLocationService trackLocationService) : ControllerBase
{
	[HttpGet("{userId}")]
	public async Task<ActionResult<TrackLocationDto>> GetTrackedLocationById(Guid userId)
	{
		return Ok(await trackLocationService.GetAllByUserIdAsync(userId));
	}

	[HttpPost]
	public async Task<ActionResult<TrackLocationDto>> Create(Guid userId,
		[FromBody] TrackLocationRequest trackLocationRequest)
	{
		if (!ModelState.IsValid) return BadRequest(ModelState);
		return Ok(await trackLocationService.CreateAsync(userId, trackLocationRequest));
	}

	[HttpPut("{userId}/{trackLocationId}")]
	public async Task<ActionResult<TrackLocationDto>> Update(Guid userId, Guid trackLocationId,
		[FromBody] TrackLocationRequest trackLocationRequest)
	{
		if (!ModelState.IsValid) return BadRequest(ModelState);
		return Ok(await trackLocationService.UpdateAsync(userId, trackLocationId, trackLocationRequest));
	}

	[HttpDelete("{userId}/{trackedLocationId}")]
	public async Task<ActionResult> Delete(Guid userId, Guid trackedLocationId)
	{
		await trackLocationService.DeleteAsync(userId, trackedLocationId);
		return NoContent();
	}
}