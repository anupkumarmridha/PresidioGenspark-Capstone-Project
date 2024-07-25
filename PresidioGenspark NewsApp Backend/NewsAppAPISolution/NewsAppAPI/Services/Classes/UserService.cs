using Google.Apis.Auth;
using NewsAppAPI.DTOs;
using NewsAppAPI.Models;
using NewsAppAPI.Repositories.Interfaces;
using NewsAppAPI.Services.Interfaces;
using Newtonsoft.Json;
using System.Text.Json;

namespace NewsAppAPI.Services.Classes
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public UserService(IUserRepository userRepository, IConfiguration configuration, HttpClient httpClient)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<UserDto> AuthenticateGoogleUserAsync(string googleToken)
        {
            var googleUserInfo = await FetchGoogleUserProfile(googleToken);

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
                    DisplayName = googleUserInfo.Name,
                    GivenName = googleUserInfo.GivenName,
                    FamilyName = googleUserInfo.FamilyName,
                    Picture = googleUserInfo.Picture,
                    Role = "User", // default role
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _userRepository.AddUserAsync(user);
            }

            // Map user data to UserDto
            var userDto = new UserDto
            {
                Email = user.Email,
                DisplayName = user.DisplayName,
                Role = user.Role,
                Picture = user.Picture
            };

            return userDto;
        }

        private async Task<GoogleUserInfo> FetchGoogleUserProfile(string accessToken)
        {
            try
            {
                var response = await _httpClient.GetAsync($"https://www.googleapis.com/oauth2/v1/userinfo?access_token={accessToken}");
                await Console.Out.WriteLineAsync($"{ response}");
                if (!response.IsSuccessStatusCode)
                    throw new Exception("Failed to fetch Google user profile");

                var jsonResponse = await response.Content.ReadAsStringAsync();

                // Manually map the JSON response to GoogleUserInfo
                var googleUserProfile = MapGoogleUserProfile(jsonResponse);

                return googleUserProfile;
            }
            catch (Exception ex)
            {
                // Handle the exception, such as logging or rethrowing
                throw new Exception("Failed to fetch Google user profile", ex);
            }
        }
        private GoogleUserInfo MapGoogleUserProfile(string jsonResponse)
        {
            var jsonObject = JsonConvert.DeserializeObject<dynamic>(jsonResponse);

            return new GoogleUserInfo
            {
                GoogleId = jsonObject.id,
                Email = jsonObject.email,
                Name = jsonObject.name,
                GivenName = jsonObject.given_name,
                FamilyName = jsonObject.family_name,
                Picture = jsonObject.picture
            };
        }
    }

}
