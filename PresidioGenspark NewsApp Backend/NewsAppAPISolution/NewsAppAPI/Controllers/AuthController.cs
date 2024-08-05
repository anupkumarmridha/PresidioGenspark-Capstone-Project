using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewsAppAPI.DTOs;
using NewsAppAPI.Services.Interfaces;

namespace NewsAppAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("MyCors")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;

        public AuthController(IUserService userService, ITokenService tokenService)
        {
            _userService = userService;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Authenticates a user using Google login and returns a JWT token.
        /// </summary>
        /// <param name="request">The Google login request containing the Google token.</param>
        /// <returns>A JWT token if authentication is successful.</returns>
        [HttpPost("google-login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.GoogleToken))
            {
                return BadRequest("Invalid request.");
            }

            var userDto = await _userService.AuthenticateGoogleUserAsync(request.GoogleToken);
            var jwtToken = _tokenService.GenerateToken(userDto);

            return Ok(new { Token = jwtToken, Profile = userDto }); // Include profile data in the response
        }
    }
}
