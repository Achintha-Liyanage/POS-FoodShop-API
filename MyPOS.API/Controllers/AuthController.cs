using Microsoft.AspNetCore.Mvc;
using MyPOS.Application.DTOs.Auth;
using MyPOS.Application.Interfaces;
using System.Threading.Tasks;

namespace MyPOS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userDto = await _authService.LoginAsync(loginRequest);

            if (userDto == null || string.IsNullOrEmpty(userDto.Token))
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }

            return Ok(userDto); // Returns UserDto which includes the token
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userDto = await _authService.RegisterAsync(registerRequest);
                // Typically, you might return a 201 Created or just a 200 OK with the user details (excluding token for register)
                return Ok(new { message = "User registered successfully.", userId = userDto.Id, username = userDto.Username, role = userDto.Role });
            }
            catch (System.ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message }); // E.g., "Username already exists."
            }
            catch (System.Exception ex)
            {
                // Log the exception: ex
                return StatusCode(500, "An error occurred during registration.");
            }
        }
    }
}
