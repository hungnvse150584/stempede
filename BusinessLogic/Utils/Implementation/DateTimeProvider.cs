using BusinessLogic.Utils.Interfaces;

namespace BusinessLogic.Utils.Implementation
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
