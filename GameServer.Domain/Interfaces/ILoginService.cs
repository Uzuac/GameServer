namespace GameServer.Domain.Interfaces
{
    public interface ILoginService
    {
        Task<bool> Login(string deviceUuid);
    }
}
