namespace API.Models.DTOs
{
    /// <summary>
    /// Dto trả dữ liệu cho client
    /// </summary>
    public class UserDto
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Dto dùng để tạo mới hoặc cập nhật User
    /// </summary>
    public class UserUpsertDto
    {
        public int? Id { get; set; } // Null khi tạo mới, có giá trị khi cập nhật
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Password { get; set; } = string.Empty; // Chỉ dùng khi tạo mới hoặc đổi mật khẩu
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
