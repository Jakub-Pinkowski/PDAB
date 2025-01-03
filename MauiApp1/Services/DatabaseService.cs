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

        public async Task<List<CustomerTotalAmount>> GetCustomerTotalAmountsAsync()
        {
            return await _database.QueryAsync<CustomerTotalAmount>(@"
        SELECT 
            c.Name AS CustomerName,
            c.Email AS CustomerEmail,
            SUM(i.TotalAmount) AS TotalAmount
        FROM 
            Customer c
        LEFT JOIN 
            [Order] o ON c.Id = o.CustomerId
        LEFT JOIN 
            Invoice i ON o.Id = i.OrderId
        GROUP BY 
            c.Name, c.Email;
    ");
        }
        public async Task<List<ProductReview>> GetProductReviewsAsync()
        {
            return await _database.QueryAsync<ProductReview>(@"
        SELECT 
            p.Name AS ProductName,
            r.Rating,
            r.Comment,
            c.Name AS CustomerName,
            cat.Name AS CategoryName
        FROM 
            Product p
        JOIN 
            Review r ON p.Id = r.ProductId
        JOIN 
            Customer c ON r.CustomerId = c.Id
        JOIN 
            Category cat ON p.CategoryId = cat.Id;
    ");
        }

        public async Task<List<OrderDetail>> GetOrderDetailsAsync()
        {
            return await _database.QueryAsync<OrderDetail>(@"
        SELECT 
            o.Id AS OrderId,
            o.OrderDate,
            p.Name AS ProductName,
            oi.Quantity,
            oi.Price,
            i.TotalAmount
        FROM 
            [Order] o
        JOIN 
            OrderItem oi ON o.Id = oi.OrderId
        JOIN 
            Product p ON oi.ProductId = p.Id
        JOIN 
            Invoice i ON o.Id = i.OrderId;
        ");
        }
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
        new Address { Street = "202 Birch St", City = "Mytown", State = "WA", ZipCode = "55667" },
        new Address { Street = "303 Cedar St", City = "Hometown", State = "CO", ZipCode = "77889" },
        new Address { Street = "404 Elm St", City = "Thistown", State = "NV", ZipCode = "99001" }
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
        new Category { Name = "Sleeping Gear", Description = "Sleeping bags and mats", IsActive = true },
        new Category { Name = "Clothing", Description = "Outdoor clothing", IsActive = true },
        new Category { Name = "Electronics", Description = "Outdoor electronics", IsActive = true }
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
        new Customer { Name = "Charlie Davis", Email = "charlie.davis@example.com" },
        new Customer { Name = "David Wilson", Email = "david.wilson@example.com" },
        new Customer { Name = "Eva Green", Email = "eva.green@example.com" },
        new Customer { Name = "Frank White", Email = "frank.white@example.com" },
        new Customer { Name = "Grace Black", Email = "grace.black@example.com" }
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
        new Discount { Code = "NEWYEAR22", Percentage = 25, ExpirationDate = DateTime.Now.AddMonths(5) },
        new Discount { Code = "BLACKFRIDAY", Percentage = 30, ExpirationDate = DateTime.Now.AddMonths(6) },
        new Discount { Code = "CYBERMONDAY", Percentage = 35, ExpirationDate = DateTime.Now.AddMonths(7) }
    };
            foreach (var discount in discounts)
            {
                await SaveItemAsync(discount);
            }

            // Populate Invoice
            var invoices = new List<Invoice>
    {
        new Invoice { OrderId = 1, TotalAmount = 60m, InvoiceDate = DateTime.Now },
        new Invoice { OrderId = 2, TotalAmount = 90m, InvoiceDate = DateTime.Now },
        new Invoice { OrderId = 3, TotalAmount = 20m, InvoiceDate = DateTime.Now },
        new Invoice { OrderId = 4, TotalAmount = 200m, InvoiceDate = DateTime.Now },
        new Invoice { OrderId = 5, TotalAmount = 50m, InvoiceDate = DateTime.Now },
        new Invoice { OrderId = 6, TotalAmount = 30m, InvoiceDate = DateTime.Now },
        new Invoice { OrderId = 7, TotalAmount = 100m, InvoiceDate = DateTime.Now },
        new Invoice { OrderId = 8, TotalAmount = 80m, InvoiceDate = DateTime.Now },
        new Invoice { OrderId = 9, TotalAmount = 40m, InvoiceDate = DateTime.Now },
        new Invoice { OrderId = 10, TotalAmount = 60m, InvoiceDate = DateTime.Now },
        new Invoice { OrderId = 11, TotalAmount = 90m, InvoiceDate = DateTime.Now },
        new Invoice { OrderId = 12, TotalAmount = 20m, InvoiceDate = DateTime.Now },
        new Invoice { OrderId = 13, TotalAmount = 200m, InvoiceDate = DateTime.Now },
        new Invoice { OrderId = 14, TotalAmount = 50m, InvoiceDate = DateTime.Now },
        new Invoice { OrderId = 15, TotalAmount = 30m, InvoiceDate = DateTime.Now },
        new Invoice { OrderId = 16, TotalAmount = 100m, InvoiceDate = DateTime.Now }
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
        new Order { CustomerId = 5, OrderDate = DateTime.Now },
        new Order { CustomerId = 6, OrderDate = DateTime.Now },
        new Order { CustomerId = 7, OrderDate = DateTime.Now },
        new Order { CustomerId = 1, OrderDate = DateTime.Now },
        new Order { CustomerId = 2, OrderDate = DateTime.Now },
        new Order { CustomerId = 3, OrderDate = DateTime.Now },
        new Order { CustomerId = 4, OrderDate = DateTime.Now },
        new Order { CustomerId = 5, OrderDate = DateTime.Now },
        new Order { CustomerId = 6, OrderDate = DateTime.Now },
        new Order { CustomerId = 7, OrderDate = DateTime.Now },
        new Order { CustomerId = 8, OrderDate = DateTime.Now },
        new Order { CustomerId = 9, OrderDate = DateTime.Now }
    };
            foreach (var order in orders)
            {
                await SaveItemAsync(order);
            }

            // Populate OrderItem
            var orderItems = new List<OrderItem>
    {
        new OrderItem { OrderId = 1, ProductId = 1, Quantity = 1, Price = 60m },
        new OrderItem { OrderId = 2, ProductId = 2, Quantity = 1, Price = 90m },
        new OrderItem { OrderId = 3, ProductId = 3, Quantity = 2, Price = 10m },
        new OrderItem { OrderId = 4, ProductId = 4, Quantity = 1, Price = 200m },
        new OrderItem { OrderId = 5, ProductId = 5, Quantity = 1, Price = 50m },
        new OrderItem { OrderId = 6, ProductId = 6, Quantity = 1, Price = 30m },
        new OrderItem { OrderId = 7, ProductId = 7, Quantity = 1, Price = 100m },
        new OrderItem { OrderId = 8, ProductId = 1, Quantity = 1, Price = 60m },
        new OrderItem { OrderId = 9, ProductId = 2, Quantity = 1, Price = 90m },
        new OrderItem { OrderId = 10, ProductId = 3, Quantity = 2, Price = 10m },
        new OrderItem { OrderId = 11, ProductId = 4, Quantity = 1, Price = 200m },
        new OrderItem { OrderId = 12, ProductId = 5, Quantity = 1, Price = 50m },
        new OrderItem { OrderId = 13, ProductId = 6, Quantity = 1, Price = 30m },
        new OrderItem { OrderId = 14, ProductId = 7, Quantity = 1, Price = 100m },
        new OrderItem { OrderId = 15, ProductId = 1, Quantity = 1, Price = 60m },
        new OrderItem { OrderId = 16, ProductId = 2, Quantity = 1, Price = 90m }
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
        new PaymentMethod { CustomerId = 5, CardNumber = "5678901234567890", ExpirationDate = DateTime.Now.AddYears(5) },
        new PaymentMethod { CustomerId = 6, CardNumber = "6789012345678901", ExpirationDate = DateTime.Now.AddYears(6) },
        new PaymentMethod { CustomerId = 7, CardNumber = "7890123456789012", ExpirationDate = DateTime.Now.AddYears(7) },
        new PaymentMethod { CustomerId = 8, CardNumber = "8901234567890123", ExpirationDate = DateTime.Now.AddYears(8) },
        new PaymentMethod { CustomerId = 9, CardNumber = "9012345678901234", ExpirationDate = DateTime.Now.AddYears(9) }
    };
            foreach (var paymentMethod in paymentMethods)
            {
                await SaveItemAsync(paymentMethod);
            }

            // Populate Product
            var products = new List<Product>
    {
        new Product { Name = "Running Shoes", Price = 69m, CategoryId = 1, SupplierId = 1 },
        new Product { Name = "Hiking Backpack", Price = 90m, CategoryId = 2, SupplierId = 2 },
        new Product { Name = "Water Bottle", Price = 10m, CategoryId = 3, SupplierId = 3 },
        new Product { Name = "Tent", Price = 200m, CategoryId = 4, SupplierId = 4 },
        new Product { Name = "Sleeping Bag", Price = 50m, CategoryId = 5, SupplierId = 5 },
        new Product { Name = "Jacket", Price = 130m, CategoryId = 6, SupplierId = 6 },
        new Product { Name = "GPS Device", Price = 300m, CategoryId = 7, SupplierId = 7 }
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
        new Review { ProductId = 5, CustomerId = 5, Rating = 4, Comment = "Comfortable sleeping bag." },
        new Review { ProductId = 6, CustomerId = 6, Rating = 4, Comment = "Warm and comfortable jacket." },
        new Review { ProductId = 7, CustomerId = 7, Rating = 5, Comment = "Very accurate GPS device." }
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
        new Shipper { Name = "Amazon Logistics", Phone = "567-890-1234" },
        new Shipper { Name = "BlueDart", Phone = "678-901-2345" },
        new Shipper { Name = "Aramex", Phone = "789-012-3456" }
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
        new ShoppingCartItem { CustomerId = 5, ProductId = 5, Quantity = 1 },
        new ShoppingCartItem { CustomerId = 6, ProductId = 6, Quantity = 1 },
        new ShoppingCartItem { CustomerId = 7, ProductId = 7, Quantity = 1 },
        new ShoppingCartItem { CustomerId = 8, ProductId = 1, Quantity = 1 },
        new ShoppingCartItem { CustomerId = 9, ProductId = 2, Quantity = 1 }
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
        new Supplier { Name = "North Face", ContactEmail = "northface@example.com" },
        new Supplier { Name = "Patagonia", ContactEmail = "patagonia@example.com" },
        new Supplier { Name = "Garmin", ContactEmail = "garmin@example.com" }
    };
            foreach (var supplier in suppliers)
            {
                await SaveItemAsync(supplier);
            }

            // Populate Transaction
            var transactions = new List<Transaction>
    {
        new Transaction { OrderId = 1, Amount = 60m, TransactionDate = DateTime.Now },
        new Transaction { OrderId = 2, Amount = 90m, TransactionDate = DateTime.Now },
        new Transaction { OrderId = 3, Amount = 20m, TransactionDate = DateTime.Now },
        new Transaction { OrderId = 4, Amount = 200m, TransactionDate = DateTime.Now },
        new Transaction { OrderId = 5, Amount = 50m, TransactionDate = DateTime.Now },
        new Transaction { OrderId = 6, Amount = 30m, TransactionDate = DateTime.Now },
        new Transaction { OrderId = 7, Amount = 100m, TransactionDate = DateTime.Now },
        new Transaction { OrderId = 8, Amount = 780m, TransactionDate = DateTime.Now },
        new Transaction { OrderId = 9, Amount = 40m, TransactionDate = DateTime.Now }
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
        new WishlistItem { CustomerId = 5, ProductId = 5 },
        new WishlistItem { CustomerId = 6, ProductId = 6 },
        new WishlistItem { CustomerId = 7, ProductId = 7 },
        new WishlistItem { CustomerId = 8, ProductId = 1 },
        new WishlistItem { CustomerId = 9, ProductId = 2 }
    };
            foreach (var wishlist in wishlists)
            {
                await SaveItemAsync(wishlist);
            }
        }

        // NOTE: Use VERY carefully
        public async Task ResetDatabaseAsync()
        {
            await DropAllTablesAsync();
            await RecreateAllTablesAsync();
            await PopulateTablesWithDummyDataAsync();
        }

        public async Task BackupDatabaseAsync(string backupPath)
        {
            try
            {
                var databasePath = _database.DatabasePath;
                if (File.Exists(databasePath))
                {
                    File.Copy(databasePath, backupPath, overwrite: true);
                    await Task.CompletedTask;
                }
                else
                {
                    throw new FileNotFoundException("Database file not found.");
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                throw new Exception($"Failed to create database backup: {ex.Message}", ex);
            }
        }
    }
}