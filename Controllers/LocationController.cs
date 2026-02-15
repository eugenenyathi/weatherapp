using Microsoft.AspNetCore.Mvc;
using weatherapp.Services.Interfaces;
using weatherapp.DataTransferObjects;
using weatherapp.Requests;

namespace weatherapp.Controllers;

[ApiController]
[Route("api/location")]
public class LocationController(ILocationService locationService) : ControllerBase
{
	//Create a new location
	[HttpPost]
	public async Task<ActionResult<LocationDto>> CreateLocation([FromBody] LocationRequest locationRequest)
	{
		if(!ModelState.IsValid) return BadRequest(ModelState);
		return Ok(await locationService.CreateAsync(locationRequest));
	}
}