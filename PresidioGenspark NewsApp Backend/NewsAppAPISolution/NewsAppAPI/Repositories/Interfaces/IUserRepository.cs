using NewsAppAPI.Models;

namespace NewsAppAPI.Repositories.Interfaces
{
    public interface IUserRepository:IRepository<int, User>
    {
        Task<User> GetUserByGoogleIdAsync(string googleId);
        Task<User> AddUserAsync(User user);
        Task<User> GetUserByEmailAsync(string email);
    }
}
