using System.Security.Cryptography;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using APIFull.Models;
using System;

namespace APIFull.Services
{
    public static class TokenService
    {
        private static List<Tuple<string, string>> _refreshTokens = new();
        public static string GenerateTokenByUser(User user)
        {
            var claims = GenerateClaims(user);
            return GenerateToken(claims);
        }

        public static string GenerateToken(IEnumerable<Claim> claims)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(Settings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public static string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public static ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Settings.Secret)),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }

        public static void SaveRefreshToken(string userName, string refreshToken)
        {
            _refreshTokens.Add(new Tuple<string, string>(userName, refreshToken));
        }

        public static string GetRefreshToken(string userName)
        {
            return _refreshTokens.FirstOrDefault(x => x.Item1.Equals(userName)).Item2;
        }

        public static void DeleteFrefreshToken(string userName, string refreshToken)
        {
            var item = _refreshTokens.FirstOrDefault(x => x.Item1.Equals(userName) && x.Item2.Equals(refreshToken));
            _refreshTokens.Remove(item);
        }

        private static IEnumerable<Claim> GenerateClaims(User user)
        {
            return new [] {
                new Claim(ClaimTypes.Name, user.UserName), // User.Identity.Name
                new Claim(ClaimTypes.Role, user.Role) // User.isInRole()
            };
        }
    }
}