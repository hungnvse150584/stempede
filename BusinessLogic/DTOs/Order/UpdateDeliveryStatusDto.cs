namespace BusinessLogic.DTOs.Order
{
    public class UpdateDeliveryStatusDto
    {
        public string DeliveryStatus { get; set; }
        public DateOnly? DeliveryDate { get; set; }
    }
}
