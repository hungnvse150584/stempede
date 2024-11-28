namespace BusinessLogic.DTOs.Lab
{
    public class ReadLabSimpleDto
    {
        public int LabId { get; set; }
        public string LabName { get; set; }
        public string? Description { get; set; }
        public string? LabFileUrl { get; set; }
    }
}
