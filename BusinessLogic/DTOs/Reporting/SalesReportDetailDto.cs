namespace BusinessLogic.DTOs.Reporting
{
    public class SalesReportDetailDto
    {
        public DateOnly OrderDate { get; set; }
        public int OrderId { get; set; }
        public string CustomerUsername { get; set; }
        public decimal OrderTotal { get; set; }
    }
}
