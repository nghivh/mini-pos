using API.Common.Helpers;
using API.Data.Interfaces;
using API.Models.DTOs;
using API.Models.Entities;
using API.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace API.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IUserContext _userContext;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper, IUserContext userContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userContext = userContext;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync(CancellationToken ct)
        {
            var users = await _unitOfWork.Repository<User>().Query().ToListAsync(ct);
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<UserDto?> GetUserByIdAsync(int id, CancellationToken ct)
        {
            var user = await _unitOfWork.Repository<User>().Query().FirstOrDefaultAsync(u => u.Id == id, ct);
            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto?> GetUserByUserNameAsync(string userName, CancellationToken ct)
        {
            var user = await _unitOfWork.Repository<User>().Query().FirstOrDefaultAsync(u => u.UserName == userName, ct);
            return _mapper.Map<UserDto>(user);
        }

        public async Task CreateAsync(UserUpsertDto userDto, CancellationToken ct)
        {
            // Kiểm tra xem username đã tồn tại chưa
            var existingUser = await _unitOfWork.Repository<User>().Query().FirstOrDefaultAsync(u => u.UserName == userDto.UserName, ct);
            if(existingUser != null)
            {
                throw new InvalidOperationException("Username này đã tồn tại.");
            }

            var user = _mapper.Map<User>(userDto);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password);

            await _unitOfWork.Repository<User>().AddAsync(user, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(UserUpsertDto userDto, CancellationToken ct)
        {
            // Kiểm tra Id đã tồn tại hay chưa
            var existingUser = await _unitOfWork.Repository<User>().Query().FirstOrDefaultAsync(u => u.Id == userDto.Id, ct);
            if(existingUser == null)
            {
                throw new InvalidOperationException("User này không tồn tại.");
            }

            var passwordChanged = !string.IsNullOrEmpty(userDto.Password);
            var isBeingDeactivated = existingUser.IsActive && !userDto.IsActive;

            // Không dùng Mapper, mà chỉ cập nhật những trường cần thay đổi
            existingUser.FullName = userDto.FullName;
            existingUser.Role = userDto.Role;
            existingUser.IsActive = userDto.IsActive;
            existingUser.UpdatedAt = DateTime.UtcNow;

            if (passwordChanged)
            {
                existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password);
            }

            _unitOfWork.Repository<User>().Update(existingUser);

            // Đổi mật khẩu hoặc khoá tài khoản => thu hồi toàn bộ refresh token của user đó.
            // Nếu không, refresh token cũ vẫn dùng được (và khi mở khoá lại, token cũ sống lại).
            if(passwordChanged || isBeingDeactivated)
            {
                var reason = isBeingDeactivated ? "Tài khoản bị khóa" : "Mật khẩu được đặt lại";
                await _unitOfWork.RevokeActiveRefreshTokensAsync(existingUser.Id, _userContext.IpAddress, reason, ct);
            }

            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}
