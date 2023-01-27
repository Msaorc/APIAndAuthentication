using Microsoft.AspNetCore.Mvc;
using APIFull.Models;
using APIFull.Repositories;
using APIFull.Services;
using Microsoft.IdentityModel.Tokens;

namespace APIFull.Controllers
{
    [ApiController]
    [Route("v1")]
    public class LoginController : ControllerBase
    {
        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<dynamic>> AuthenticateAsync([FromBody] User bodyUser)
        {
            var user = UserRepository.Get(bodyUser.UserName, bodyUser.PassWord);

            if (user == null)
            {
                return NotFound(new {message = "Invalid username or password"});
            }

            var token = TokenService.GenerateTokenByUser(user);
            var refreshToken = TokenService.GenerateRefreshToken();
            TokenService.SaveRefreshToken(user.UserName, refreshToken);

            user.PassWord = "";

            return new {
                message = "Token is generate with success",
                user = user,
                token = token,
                refresToken = refreshToken
            };
        }

        [HttpPost]
        [Route("refreshToken")]
        public IActionResult RefreshToken([FromBody] TokenObject tokenObject)
        {
            var principal = TokenService.GetPrincipalFromExpiredToken(tokenObject.Token);
            var username = principal.Identity.Name;
            var savedRefreshToken = TokenService.GetRefreshToken(username);
            if (savedRefreshToken != tokenObject.RefreshToken)
            {
                throw new SecurityTokenException("Invalid token");
            }

            var newJWtToken = TokenService.GenerateToken(principal.Claims);
            var newRefreshToken = TokenService.GenerateRefreshToken();
            TokenService.DeleteFrefreshToken(username, tokenObject.RefreshToken);
            TokenService.SaveRefreshToken(username, newRefreshToken);

            return new ObjectResult(new{
                token = newJWtToken,
                refreshToken = newRefreshToken
            });
        }
    }
}