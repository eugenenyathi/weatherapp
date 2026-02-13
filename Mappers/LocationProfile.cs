using AutoMapper;
using weatherapp.DataTransferObjects;
using weatherapp.Entities;
using weatherapp.Requests;

namespace weatherapp.Mappers;

public class LocationProfile : Profile
{
	public LocationProfile()
	{
		CreateMap<Location, LocationDto>()
			.ReverseMap();

		CreateMap<LocationRequest, Location>();
	}
}