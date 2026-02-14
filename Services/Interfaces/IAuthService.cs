using weatherapp.DataTransferObjects;
using weatherapp.Requests;

namespace weatherapp.Services.Interfaces;

public interface IAuthService
{
    Task<UserDto> Register(RegisterRequest request);
    Task<UserDto> Update(Guid userId, UpdateRequest request);
}
