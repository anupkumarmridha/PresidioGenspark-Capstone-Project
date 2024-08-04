using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.IdentityModel.Tokens;
using NewsAppAPI.DTOs;
using NewsAppAPI.Models;
using NewsAppAPI.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NewsAppAPI.Services.Classes
{
    public class TokenService : ITokenService
    {
        private readonly string _secretKey;
        private readonly SymmetricSecurityKey _key;

        private static async Task<string> GetSecretAsync(SecretClient secretClient, string secretName)
        {
            var secret = await secretClient.GetSecretAsync(secretName);
            return secret.Value.Value;
        }

        public TokenService(IConfiguration configuration)
        {
            //_secretKey = configuration.GetSection("TokenKey").GetSection("JWT").Value.ToString();
            var keyVaultName = configuration["KeyVault:Name"];
            var kvUri = $"https://{keyVaultName}.vault.azure.net/";


            var secretClient = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());

            _secretKey = GetSecretAsync(secretClient, "JWT").Result;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        }
        public string GenerateToken(UserDto user)
        {
            var claims = new List<Claim>(){
                new Claim(ClaimTypes.Name, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };
            var credentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);
            var myToken = new JwtSecurityToken(null, null, claims, expires: DateTime.Now.AddDays(20), signingCredentials: credentials);
            string token = new JwtSecurityTokenHandler().WriteToken(myToken);
            return token;
        }
    }
}
