namespace BusinessLogic.DTOs.Reporting
{
    public class SalesReportDto
    {
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
        public decimal TotalSales { get; set; }
        public int TotalOrders { get; set; }
        public List<SalesReportDetailDto> Details { get; set; }
    }
}
