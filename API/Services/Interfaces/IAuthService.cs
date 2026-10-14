using API.Models.DTOs.Auth;
using API.Models.Entities;

namespace API.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(string username, string password, string? device, string ip, CancellationToken ct);
        Task<AuthResponse> RefreshAsync(string refreshToken, string ip, CancellationToken ct);
        Task LogoutAsync(string refreshToken, string ip, CancellationToken ct);
        Task LogoutAllAsync(string username, string ip, CancellationToken ct);
        Task<CurrentUser?> CurrentUserAsync(string username);
    }
}
