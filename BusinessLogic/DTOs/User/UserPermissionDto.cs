namespace BusinessLogic.DTOs.User
{
    public class UserPermissionDto
    {
        public int PermissionId { get; set; }
        public string PermissionName { get; set; }
        public string Description { get; set; }
        public string AssignedBy { get; set; }
    }
}
