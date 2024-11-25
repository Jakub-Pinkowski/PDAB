using SQLite;

namespace MauiApp1.Models
{
    public class Order
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int CustomerId { get; set; } // Foreign key to Customer
        public DateTime OrderDate { get; set; }
    }
}