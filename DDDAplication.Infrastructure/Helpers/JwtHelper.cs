using DDDAplication.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DDDAplication.Infrastructure.Helpers
{
    public static class JwtHelper
    {
        public static string GenerateToken(ApplicationUser user, IConfiguration configuration)
        {
            try
            {
                var jwtSettings = configuration.GetSection("JwtSettings");
                var secretKey = jwtSettings["Secret"];
                var issuer = jwtSettings["Issuer"];
                var audience = jwtSettings["Audience"];
                var tokenLifetime = int.Parse(jwtSettings["TokenLifetimeMinutes"]);

                if (string.IsNullOrWhiteSpace(secretKey) || string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience))
                {
                    throw new Exception("JWT configuration is invalid.");
                }

                var claims = new[]
                {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(tokenLifetime),
                    signingCredentials: creds
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                throw new Exception($"An error occurred while generating the JWT token: {ex.Message}", ex);
            }
        }
        public static string GenerateRoleToken(Role rol, IConfiguration configuration)
        {
            try
            {
                // Read JWT settings from configuration
                var jwtSettings = configuration.GetSection("JwtSettings");
                var secretKey = jwtSettings["Secret"];
                var issuer = jwtSettings["Issuer"];
                var audience = jwtSettings["Audience"];
                var tokenLifetime = int.Parse(jwtSettings["TokenLifetimeMinutes"]);

                // Validate settings
                if (string.IsNullOrWhiteSpace(secretKey) || string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience))
                {
                    throw new Exception("JWT configuration is invalid.");
                }

                // Define claims
                var claims = new[]
                {
            new Claim(JwtRegisteredClaimNames.Sub, rol.Name), // The role name
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, rol.Name) // Name of the role
        };

                // Create security key and credentials
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                // Create token
                var token = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(tokenLifetime),
                    signingCredentials: creds
                );

                // Return the serialized token
                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                throw new Exception($"An error occurred while generating the Role JWT token: {ex.Message}", ex);
            }
        }

        public static ClaimsPrincipal GetPrincipalFromExpiredToken(string token, IConfiguration configuration)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(configuration["JwtSettings:Secret"]);

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                }, out var validatedToken);

                return principal;
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}

