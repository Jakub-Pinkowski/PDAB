using SQLite;

namespace MauiApp1.Models
{
    public class PaymentMethod
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int CustomerId { get; set; } // Foreign key to Customer
        public string CardNumber { get; set; } = string.Empty;
        public DateTime ExpirationDate { get; set; }
    }
}