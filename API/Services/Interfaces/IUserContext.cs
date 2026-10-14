namespace API.Services.Interfaces
{
    public interface IUserContext
    {
        int? UserId { get; }
        string? UserName { get; }
        string IpAddress { get; }
        string MachineName { get; }
    }
}
