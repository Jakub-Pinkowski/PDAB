using MauiApp1.Models;
using MauiApp1.Services;
using Microsoft.Maui.Controls;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MauiApp1
{
    public partial class ProductsPage : ContentPage
    {
        private readonly DatabaseService _databaseService;

        public ProductsPage(DatabaseService databaseService)
        {
            InitializeComponent();
            _databaseService = databaseService;
            InitializeDatabaseAsync().ConfigureAwait(false);
        }

        private async Task InitializeDatabaseAsync()
        {
            await _databaseService.CreateTableAsync<Product>();

            var products = await _databaseService.GetItemsAsync<Product>();
            if (products.Count == 0)
            {
                var dummyProducts = new List<Product>
                {
                    new Product { Name = "Product 1", Price = 10.99m },
                    new Product { Name = "Product 2", Price = 20.99m },
                    new Product { Name = "Product 3", Price = 30.99m },
                    new Product { Name = "Product 4", Price = 40.99m },
                    new Product { Name = "Product 5", Price = 50.99m }
                };

                foreach (var product in dummyProducts)
                {
                    await _databaseService.SaveItemAsync(product);
                }
            }

            LoadProductsAsync();
        }

        private async void LoadProductsAsync()
        {
            var products = await _databaseService.GetItemsAsync<Product>();
            ProductsCollectionView.ItemsSource = products;
        }
    }
}