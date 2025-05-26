using Microsoft.Extensions.Configuration; // For JWT settings
using Microsoft.IdentityModel.Tokens; // For SymmetricSecurityKey, SigningCredentials
using MyPOS.Application.DTOs.Auth;
using MyPOS.Application.Interfaces;
using MyPOS.Domain.Interfaces; // To potentially interact with IRepository<User>
using MyPOS.Domain.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt; // For JwtSecurityTokenHandler
using System.Linq;
using System.Security.Claims; // For Claims
using System.Text; // For Encoding
using System.Threading.Tasks;

namespace MyPOS.Application.Services
{
    public class AuthService : IAuthService
    {
        // Temporary in-memory user store for demonstration as migrations are an issue.
        // Replace with IRepository<User> and database interaction in a real scenario.
        private static readonly List<User> _users = new List<User>();
        private readonly IConfiguration _configuration;
        private readonly IRepository<User> _userRepository; // Inject if using DB

        public AuthService(IConfiguration configuration, IRepository<User> userRepository)
        {
            _configuration = configuration;
            _userRepository = userRepository; // Will use this primarily for interaction

            // Seed a default admin user if none exist (for testing, as migrations are off)
             InitializeAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private async Task InitializeAsync()
        {
            if (!(await _userRepository.GetAllAsync()).Any(u => u.Username == "admin"))
            {
                 var adminUser = new User
                {
                    Username = "admin",
                    // Store a hash for "adminpassword"
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("adminpassword"),
                    Role = "Admin"
                };
                await _userRepository.AddAsync(adminUser); // Add to DB
                //_users.Add(adminUser); // Also add to in-memory if using hybrid for some reason
            }
        }


        public async Task<UserDto> LoginAsync(LoginRequestDto loginRequest)
        {
            var user = (await _userRepository.GetAllAsync()).FirstOrDefault(u => u.Username == loginRequest.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
            {
                // Authentication failed
                return null;
            }

            // Authentication successful, generate JWT
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddHours(1), // Token expiration
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Role = user.Role,
                Token = tokenString
            };
        }

        public async Task<UserDto> RegisterAsync(RegisterRequestDto registerRequest)
        {
            if (string.IsNullOrWhiteSpace(registerRequest.Password))
            {
                throw new ApplicationException("Password is required.");
            }
             if ((await _userRepository.GetAllAsync()).Any(u => u.Username == registerRequest.Username))
            {
                throw new ApplicationException("Username already exists.");
            }

            var user = new User
            {
                Username = registerRequest.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password), // Hash password
                Role = string.IsNullOrEmpty(registerRequest.Role) ? "User" : registerRequest.Role // Default to "User"
            };

            await _userRepository.AddAsync(user);
            // _users.Add(user); // If also maintaining an in-memory list

            // For simplicity, not generating a token on registration or returning full UserDto with token
            // Client should login after registration to get a token.
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Role = user.Role
                // Token will be empty here
            };
        }
    }
}
