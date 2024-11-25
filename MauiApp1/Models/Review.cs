using SQLite;

namespace MauiApp1.Models
{
    public class Review
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int ProductId { get; set; } // Foreign key to Product
        public int CustomerId { get; set; } // Foreign key to Customer
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
    }
}