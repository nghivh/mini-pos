namespace API.Services.Interfaces
{
    public interface IUserContext
    {
        string IpAddress { get; }
        string MachineName { get; }
    }
}
