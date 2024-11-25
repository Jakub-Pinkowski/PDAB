namespace MauiApp1.Models
{
    public class ProductReview
    {
        public string ProductName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
    }
}