namespace BusinessLogic.Constants
{
    /// <summary>
    /// Defines the possible statuses for a shopping cart.
    /// </summary>
    public static class CartStatusConstants
    {
        /// <summary>
        /// The cart is active and can be modified.
        /// </summary>
        public const string Active = "Active";

        /// <summary>
        /// The cart has been checked out and cannot be modified.
        /// </summary>
        public const string CheckedOut = "CheckedOut";
    }
}