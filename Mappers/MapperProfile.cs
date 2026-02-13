using AutoMapper;
using weatherapp.DataTransferObjects;
using weatherapp.Entities;
using weatherapp.Requests;

namespace weatherapp.Mappers;

public class MapperProfile : Profile
{
	public MapperProfile()
	{
		CreateMap<Location, LocationDto>();
			
		CreateMap<LocationRequest, Location>();

		CreateMap<TrackLocation, TrackLocationDto>();

		CreateMap<TrackLocationRequest, TrackLocation>();

		CreateMap<UserPreference, UserPreferenceDto>();

		CreateMap<UserPreferenceRequest, UserPreference>();
	}
}