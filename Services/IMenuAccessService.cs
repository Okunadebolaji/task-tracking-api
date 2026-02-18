namespace TaskTrackingApi.Models;
public interface IMenuAccessService
{
    Task<bool> CanViewMenuAsync(int userId, string menuKey);
}

