using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;
using MauiApp1.Models;

namespace MauiApp1.Services
{
    public class DatabaseService
    {
        private readonly SQLiteAsyncConnection _database;

        public DatabaseService(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
        }

        public Task CreateTableAsync<T>() where T : new()
        {
            return _database.CreateTableAsync<T>();
        }

        public Task<List<T>> GetItemsAsync<T>() where T : new()
        {
            return _database.Table<T>().ToListAsync();
        }

        public Task<int> SaveItemAsync<T>(T item) where T : new()
        {
            var propertyInfo = item?.GetType().GetProperty("Id");
            if (propertyInfo != null)
            {
                var value = propertyInfo.GetValue(item);
                if (value != null && (int)value != 0)
                {
                    return _database.UpdateAsync(item);
                }
            }
            return _database.InsertAsync(item);
        }

        public Task<int> DeleteItemAsync<T>(T item) where T : new()
        {
            return _database.DeleteAsync(item);
        }

        // NOTE: Use VERY carefully
        public async Task DropAllTablesAsync()
        {
            await _database.DropTableAsync<Address>();
            await _database.DropTableAsync<Category>();
            await _database.DropTableAsync<Customer>();
            await _database.DropTableAsync<Discount>();
            await _database.DropTableAsync<Invoice>();
            await _database.DropTableAsync<Order>();
            await _database.DropTableAsync<OrderItem>();
            await _database.DropTableAsync<PaymentMethod>();
            await _database.DropTableAsync<Product>();
            await _database.DropTableAsync<Review>();
            await _database.DropTableAsync<Shipper>();
            await _database.DropTableAsync<ShoppingCartItem>();
            await _database.DropTableAsync<Supplier>();
            await _database.DropTableAsync<Transaction>();
            await _database.DropTableAsync<WishlistItem>();
        }

        // NOTE: Use VERY carefully
        public async Task RecreateAllTablesAsync()
        {
            await CreateTableAsync<Address>();
            await CreateTableAsync<Category>();
            await CreateTableAsync<Customer>();
            await CreateTableAsync<Discount>();
            await CreateTableAsync<Invoice>();
            await CreateTableAsync<Order>();
            await CreateTableAsync<OrderItem>();
            await CreateTableAsync<PaymentMethod>();
            await CreateTableAsync<Product>();
            await CreateTableAsync<Review>();
            await CreateTableAsync<Shipper>();
            await CreateTableAsync<ShoppingCartItem>();
            await CreateTableAsync<Supplier>();
            await CreateTableAsync<Transaction>();
            await CreateTableAsync<WishlistItem>();
        }

        public async Task PopulateTablesWithDummyDataAsync()
        {
            // Populate Address
            var addresses = new List<Address>
    {
        new Address { Street = "123 Main St", City = "Anytown", State = "CA", ZipCode = "12345" },
        new Address { Street = "456 Oak St", City = "Othertown", State = "TX", ZipCode = "67890" },
        new Address { Street = "789 Pine St", City = "Sometown", State = "NY", ZipCode = "11223" },
        new Address { Street = "101 Maple St", City = "Yourtown", State = "FL", ZipCode = "33445" },
        new Address { Street = "202 Birch St", City = "Mytown", State = "WA", ZipCode = "55667" }
    };
            foreach (var address in addresses)
            {
                await SaveItemAsync(address);
            }

            // Populate Category
            var categories = new List<Category>
    {
        new Category { Name = "Footwear", Description = "Shoes and related items", IsActive = true },
        new Category { Name = "Backpacks", Description = "Various types of backpacks", IsActive = true },
        new Category { Name = "Accessories", Description = "Accessories for outdoor activities", IsActive = true },
        new Category { Name = "Camping Gear", Description = "Equipment for camping", IsActive = true },
        new Category { Name = "Sleeping Gear", Description = "Sleeping bags and mats", IsActive = true }
    };
            foreach (var category in categories)
            {
                await SaveItemAsync(category);
            }

            // Populate Customer
            var customers = new List<Customer>
    {
        new Customer { Name = "John Doe", Email = "john.doe@example.com" },
        new Customer { Name = "Jane Smith", Email = "jane.smith@example.com" },
        new Customer { Name = "Alice Johnson", Email = "alice.johnson@example.com" },
        new Customer { Name = "Bob Brown", Email = "bob.brown@example.com" },
        new Customer { Name = "Charlie Davis", Email = "charlie.davis@example.com" }
    };
            foreach (var customer in customers)
            {
                await SaveItemAsync(customer);
            }

            // Populate Discount
            var discounts = new List<Discount>
    {
        new Discount { Code = "SUMMER21", Percentage = 10, ExpirationDate = DateTime.Now.AddMonths(1) },
        new Discount { Code = "WINTER21", Percentage = 15, ExpirationDate = DateTime.Now.AddMonths(2) },
        new Discount { Code = "SPRING21", Percentage = 5, ExpirationDate = DateTime.Now.AddMonths(3) },
        new Discount { Code = "FALL21", Percentage = 20, ExpirationDate = DateTime.Now.AddMonths(4) },
        new Discount { Code = "NEWYEAR22", Percentage = 25, ExpirationDate = DateTime.Now.AddMonths(5) }
    };
            foreach (var discount in discounts)
            {
                await SaveItemAsync(discount);
            }

            // Populate Invoice
            var invoices = new List<Invoice>
    {
        new Invoice { OrderId = 1, TotalAmount = 59.99m, InvoiceDate = DateTime.Now },
        new Invoice { OrderId = 2, TotalAmount = 89.99m, InvoiceDate = DateTime.Now },
        new Invoice { OrderId = 3, TotalAmount = 19.98m, InvoiceDate = DateTime.Now },
        new Invoice { OrderId = 4, TotalAmount = 199.99m, InvoiceDate = DateTime.Now },
        new Invoice { OrderId = 5, TotalAmount = 49.99m, InvoiceDate = DateTime.Now }
    };
            foreach (var invoice in invoices)
            {
                await SaveItemAsync(invoice);
            }

            // Populate Order
            var orders = new List<Order>
    {
        new Order { CustomerId = 1, OrderDate = DateTime.Now },
        new Order { CustomerId = 2, OrderDate = DateTime.Now },
        new Order { CustomerId = 3, OrderDate = DateTime.Now },
        new Order { CustomerId = 4, OrderDate = DateTime.Now },
        new Order { CustomerId = 5, OrderDate = DateTime.Now }
    };
            foreach (var order in orders)
            {
                await SaveItemAsync(order);
            }

            // Populate OrderItem
            var orderItems = new List<OrderItem>
    {
        new OrderItem { OrderId = 1, ProductId = 1, Quantity = 1, Price = 59.99m },
        new OrderItem { OrderId = 2, ProductId = 2, Quantity = 1, Price = 89.99m },
        new OrderItem { OrderId = 3, ProductId = 3, Quantity = 2, Price = 9.99m },
        new OrderItem { OrderId = 4, ProductId = 4, Quantity = 1, Price = 199.99m },
        new OrderItem { OrderId = 5, ProductId = 5, Quantity = 1, Price = 49.99m }
    };
            foreach (var orderItem in orderItems)
            {
                await SaveItemAsync(orderItem);
            }

            // Populate PaymentMethod
            var paymentMethods = new List<PaymentMethod>
    {
        new PaymentMethod { CustomerId = 1, CardNumber = "1234567890123456", ExpirationDate = DateTime.Now.AddYears(1) },
        new PaymentMethod { CustomerId = 2, CardNumber = "2345678901234567", ExpirationDate = DateTime.Now.AddYears(2) },
        new PaymentMethod { CustomerId = 3, CardNumber = "3456789012345678", ExpirationDate = DateTime.Now.AddYears(3) },
        new PaymentMethod { CustomerId = 4, CardNumber = "4567890123456789", ExpirationDate = DateTime.Now.AddYears(4) },
        new PaymentMethod { CustomerId = 5, CardNumber = "5678901234567890", ExpirationDate = DateTime.Now.AddYears(5) }
    };
            foreach (var paymentMethod in paymentMethods)
            {
                await SaveItemAsync(paymentMethod);
            }

            // Populate Product
            var products = new List<Product>
    {
        new Product { Name = "Running Shoes", Price = 59.99m, CategoryId = 1, SupplierId = 1 },
        new Product { Name = "Hiking Backpack", Price = 89.99m, CategoryId = 2, SupplierId = 2 },
        new Product { Name = "Water Bottle", Price = 9.99m, CategoryId = 3, SupplierId = 3 },
        new Product { Name = "Tent", Price = 199.99m, CategoryId = 4, SupplierId = 4 },
        new Product { Name = "Sleeping Bag", Price = 49.99m, CategoryId = 5, SupplierId = 5 }
    };
            foreach (var product in products)
            {
                await SaveItemAsync(product);
            }

            // Populate Review
            var reviews = new List<Review>
    {
        new Review { ProductId = 1, CustomerId = 1, Rating = 5, Comment = "Great running shoes!" },
        new Review { ProductId = 2, CustomerId = 2, Rating = 4, Comment = "Very spacious backpack." },
        new Review { ProductId = 3, CustomerId = 3, Rating = 3, Comment = "Decent water bottle." },
        new Review { ProductId = 4, CustomerId = 4, Rating = 5, Comment = "Excellent tent!" },
        new Review { ProductId = 5, CustomerId = 5, Rating = 4, Comment = "Comfortable sleeping bag." }
    };
            foreach (var review in reviews)
            {
                await SaveItemAsync(review);
            }

            // Populate Shipper
            var shippers = new List<Shipper>
    {
        new Shipper { Name = "UPS", Phone = "123-456-7890" },
        new Shipper { Name = "FedEx", Phone = "234-567-8901" },
        new Shipper { Name = "DHL", Phone = "345-678-9012" },
        new Shipper { Name = "USPS", Phone = "456-789-0123" },
        new Shipper { Name = "Amazon Logistics", Phone = "567-890-1234" }
    };
            foreach (var shipper in shippers)
            {
                await SaveItemAsync(shipper);
            }

            // Populate ShoppingCart
            var shoppingCarts = new List<ShoppingCartItem>
    {
        new ShoppingCartItem { CustomerId = 1, ProductId = 1, Quantity = 1 },
        new ShoppingCartItem { CustomerId = 2, ProductId = 2, Quantity = 1 },
        new ShoppingCartItem { CustomerId = 3, ProductId = 3, Quantity = 2 },
        new ShoppingCartItem { CustomerId = 4, ProductId = 4, Quantity = 1 },
        new ShoppingCartItem { CustomerId = 5, ProductId = 5, Quantity = 1 }
    };
            foreach (var shoppingCart in shoppingCarts)
            {
                await SaveItemAsync(shoppingCart);
            }

            // Populate Supplier
            var suppliers = new List<Supplier>
    {
        new Supplier { Name = "Nike", ContactEmail = "nike@example.com" },
        new Supplier { Name = "Adidas", ContactEmail = "adidas@example.com" },
        new Supplier { Name = "CamelBak", ContactEmail = "camelbak@example.com" },
        new Supplier { Name = "Coleman", ContactEmail = "coleman@example.com" },
        new Supplier { Name = "North Face", ContactEmail = "northface@example.com" }
    };
            foreach (var supplier in suppliers)
            {
                await SaveItemAsync(supplier);
            }

            // Populate Transaction
            var transactions = new List<Transaction>
    {
        new Transaction { OrderId = 1, Amount = 59.99m, TransactionDate = DateTime.Now },
        new Transaction { OrderId = 2, Amount = 89.99m, TransactionDate = DateTime.Now },
        new Transaction { OrderId = 3, Amount = 19.98m, TransactionDate = DateTime.Now },
        new Transaction { OrderId = 4, Amount = 199.99m, TransactionDate = DateTime.Now },
        new Transaction { OrderId = 5, Amount = 49.99m, TransactionDate = DateTime.Now }
    };
            foreach (var transaction in transactions)
            {
                await SaveItemAsync(transaction);
            }

            // Populate Wishlist
            var wishlists = new List<WishlistItem>
    {
        new WishlistItem { CustomerId = 1, ProductId = 1 },
        new WishlistItem { CustomerId = 2, ProductId = 2 },
        new WishlistItem { CustomerId = 3, ProductId = 3 },
        new WishlistItem { CustomerId = 4, ProductId = 4 },
        new WishlistItem { CustomerId = 5, ProductId = 5 }
    };
            foreach (var wishlist in wishlists)
            {
                await SaveItemAsync(wishlist);
            }
        }

        public async Task ResetDatabaseAsync()
        {
            await DropAllTablesAsync();
            await RecreateAllTablesAsync();
            await PopulateTablesWithDummyDataAsync();
        }
    }
}