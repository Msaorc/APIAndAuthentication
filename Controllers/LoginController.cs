using Microsoft.AspNetCore.Mvc;
using APIFull.Models;
using APIFull.Repositories;
using APIFull.Services;

namespace APIFull.Controllers
{
    [ApiController]
    [Route("v1")]
    public class LoginController : ControllerBase
    {
        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<dynamic>> AuthenticateAsync(User bodyUser)
        {
            var user = UserRepository.Get(bodyUser.UserName, bodyUser.PassWord);

            if (user == null)
            {
                return NotFound(new {message = "Invalid username or password"});
            }

            var token = TokenService.GenerateToken(user);

            user.PassWord = "";

            return new {
                message = "Token is generate with success",
                user = user,
                token = token
            };
        }
    }
}