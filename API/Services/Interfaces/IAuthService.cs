using API.Models.DTOs.Auth;
using API.Models.Entities;

namespace API.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(string username, string password, string? device, string ip, CancellationToken ct);
        Task<AuthResponse> RefreshAsync(string refreshToken, string expiredAccessToken);
        Task<CurrentUser?> CurrentUserAsync(string username);
    }
}
