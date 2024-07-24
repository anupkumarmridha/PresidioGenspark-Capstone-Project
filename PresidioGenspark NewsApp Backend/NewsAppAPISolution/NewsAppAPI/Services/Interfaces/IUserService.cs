using NewsAppAPI.DTOs;

namespace NewsAppAPI.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> AuthenticateGoogleUserAsync(string googleToken);
    }
}
