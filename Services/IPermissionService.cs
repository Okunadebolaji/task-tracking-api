public interface IPermissionService
{
    Task<bool> HasPermissionAsync(int userId, string permissionKey);
}
