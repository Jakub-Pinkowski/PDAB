using SQLite;

namespace MauiApp1.Models
{
    public class Transaction
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int OrderId { get; set; } // Foreign key to Order
        public DateTime TransactionDate { get; set; }
        public decimal Amount { get; set; }
    }
}