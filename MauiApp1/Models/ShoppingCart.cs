using SQLite;

namespace MauiApp1.Models
{
    public class ShoppingCartItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int CustomerId { get; set; } // Foreign key to Customer
        public int ProductId { get; set; } // Foreign key to Product
        public int Quantity { get; set; }
    }
}