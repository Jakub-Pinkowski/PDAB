using SQLite;

namespace MauiApp1.Models
{
    public class Invoice
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int OrderId { get; set; } // Foreign key to Order
        public DateTime InvoiceDate { get; set; }
        public decimal TotalAmount { get; set; }
    }
}