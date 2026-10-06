using API.Models.DTOs;
using API.Models.Entities;

namespace API.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync(CancellationToken ct);
        Task<UserDto?> GetUserByIdAsync(int id, CancellationToken ct);
        Task<UserDto?> GetUserByUserNameAsync(string userName, CancellationToken ct);
        Task CreateAsync(UserUpsertDto userDto, CancellationToken ct);
        Task UpdateAsync(UserUpsertDto userDto, CancellationToken ct);
    }
}
