namespace BusinessLogic.Auth.Helpers.Interfaces
{
    public interface IAssignMissingPermissions
    {
        Task AssignMissingPermissionsAsync(int userId, List<string> roleNames);
    }
}
