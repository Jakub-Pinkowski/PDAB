using SQLite;

namespace MauiApp1.Models
{
    public class Product
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        [NotNull]
        public decimal Price { get; set; }
        public int CategoryId { get; set; } // Foreign key to Category
        public int SupplierId { get; set; } // Foreign key to Supplier
    }
}