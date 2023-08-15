using OffChainStorage.Services;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OffChainStorage.Services
{
   public class JwtService : IJwtService
   {
      private readonly string _secretKey;
      private readonly string _issuer;
      private readonly string _audience;

      public JwtService(string secretKey, string issuer, string audience)
      {
         _secretKey = secretKey;
         _issuer = issuer;
         _audience = audience;
      }

      // Generates a JWT token using the provided user ID
      public string GenerateToken(string userId)
      {
         // Define the claims for the token, including user ID
         var claims = new[]
         {
         new Claim(ClaimTypes.NameIdentifier, userId)
      };

         // Create a symmetric security key from the secret key
         var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));

         // Create signing credentials using the key and HMACSHA256 algorithm
         var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

         // Create a JWT token with specified parameters
         var token = new JwtSecurityToken(
            _issuer,               // Token issuer
            _audience,             // Token audience
            claims,                // Claims to include in the token
            expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(10)),  // Expiry time
            signingCredentials: creds  // Signing credentials
         );

         // Write the token as a string
         return new JwtSecurityTokenHandler().WriteToken(token);
      }
   }
}