namespace MyRazorPages.Models
{
    public class Cart
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }

        public Cart()
        {
        }

        public Cart(int productId, int quantity)
        {
            ProductId = productId;
            Quantity = quantity;
        }
    }
}
