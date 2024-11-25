using SQLite;

namespace MauiApp1.Models
{
    public class Discount
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public decimal Percentage { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}