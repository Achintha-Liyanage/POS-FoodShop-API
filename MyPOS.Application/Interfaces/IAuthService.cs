using MyPOS.Application.DTOs.Auth;
using System.Threading.Tasks;

namespace MyPOS.Application.Interfaces
{
    public interface IAuthService
    {
        Task<UserDto> LoginAsync(LoginRequestDto loginRequest);
        Task<UserDto> RegisterAsync(RegisterRequestDto registerRequest);
    }
}
