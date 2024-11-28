namespace BusinessLogic.DTOs.User
{
    public class ReadUserDto
    {
        public int UserId { get; set; }
        public string? FullName { get; set; }
        public string? Username { get; set; }
        public string? Status { get; set; } // "Active" or "Banned"
        public bool IsExternal { get; set; }
        public string? ExternalProvider { get; set; }
        public List<string>? Roles { get; set; }
    }
}
