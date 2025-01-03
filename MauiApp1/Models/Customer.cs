using SQLite;

namespace MauiApp1.Models
{
    public class Customer
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        [Unique, NotNull]
        public string Email { get; set; } = string.Empty;
    }
}