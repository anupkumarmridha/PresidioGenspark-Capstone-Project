using Google.Apis.Auth;
using NewsAppAPI.DTOs;
using NewsAppAPI.Models;
using NewsAppAPI.Repositories.Interfaces;
using NewsAppAPI.Services.Interfaces;

namespace NewsAppAPI.Services.Classes
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public UserService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<UserDto> AuthenticateGoogleUserAsync(string googleToken)
        {
            var googleUserInfo = await VerifyGoogleToken(googleToken);

            if (googleUserInfo == null)
                throw new Exception("Invalid Google token");

            // Check if the user exists in the database
            var user = await _userRepository.GetUserByGoogleIdAsync(googleUserInfo.GoogleId);

            if (user == null)
            {
                // If user does not exist, create a new user
                user = new User
                {
                    GoogleId = googleUserInfo.GoogleId,
                    Email = googleUserInfo.Email,
                    DisplayName = googleUserInfo.DisplayName,
                    Role = "User" // default role
                };
                await _userRepository.AddUserAsync(user);
            }

            // Map user data to UserDto
            var userDto = new UserDto
            {
                Email = user.Email,
                DisplayName = user.DisplayName,
                Role = user.Role
            };

            return userDto;
        }

        private async Task<GoogleUserInfo> VerifyGoogleToken(string googleToken)
        {
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(googleToken, new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _configuration["GoogleAuthSettings:Google:ClientId"] }
                });

                return new GoogleUserInfo
                {
                    GoogleId = payload.Subject,
                    Email = payload.Email,
                    DisplayName = payload.Name
                };
            }
            catch (Exception ex)
            {
                // Handle the exception, such as logging or rethrowing
                throw new Exception("Invalid Google token", ex);
            }
        }
    }
}
