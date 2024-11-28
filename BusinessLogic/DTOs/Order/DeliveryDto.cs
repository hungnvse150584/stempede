namespace BusinessLogic.DTOs.Order
{
    public class DeliveryDto
    {
        public int DeliveryId { get; set; }
        public string DeliveryStatus { get; set; }
        public DateOnly? DeliveryDate { get; set; }
    }
}
