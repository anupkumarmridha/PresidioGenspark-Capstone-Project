using NewsAppAPI.DTOs;
using NewsAppAPI.Models;

namespace NewsAppAPI.Services.Interfaces
{
    public interface ITokenService
    {
        public string GenerateToken(UserDto user);
    }
}
