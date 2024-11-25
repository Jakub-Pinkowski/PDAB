using MauiApp1.Models;
using MauiApp1.Services;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MauiApp1
{
    public partial class ProductsPage : ContentPage
    {
        private readonly DatabaseService _databaseService;
        private Product? _editingProduct;

        public ProductsPage(DatabaseService databaseService)
        {
            InitializeComponent();
            _databaseService = databaseService;
            LoadProductsAsync();
        }

        private async void LoadProductsAsync()
        {
            var products = await _databaseService.GetItemsAsync<Product>();
            ProductsCollectionView.ItemsSource = products;
        }

        private async void OnAddProductClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameEntry.Text) || NameEntry.Text.Length < 2 ||
                string.IsNullOrWhiteSpace(PriceEntry.Text) || !decimal.TryParse(PriceEntry.Text, out _))
            {
                await DisplayAlert("Validation Error", "Name must be at least 2 characters long and Price must be a valid number.", "OK");
                return;
            }

            if (_editingProduct == null)
            {
                var newProduct = new Product
                {
                    Name = NameEntry.Text,
                    Price = decimal.Parse(PriceEntry.Text)
                };

                await _databaseService.SaveItemAsync(newProduct);
            }
            else
            {
                _editingProduct.Name = NameEntry.Text;
                _editingProduct.Price = decimal.Parse(PriceEntry.Text);
                await _databaseService.SaveItemAsync(_editingProduct);
                _editingProduct = null;
            }

            LoadProductsAsync();

            // Reset the input fields
            NameEntry.Text = string.Empty;
            PriceEntry.Text = string.Empty;
        }

        private async void OnDeleteProductClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var product = button?.CommandParameter as Product;

            if (product != null)
            {
                bool confirm = await DisplayAlert("Confirm Delete", $"Are you sure you want to delete {product.Name}?", "Yes", "No");
                if (confirm)
                {
                    await _databaseService.DeleteItemAsync(product);
                    LoadProductsAsync();
                }
            }
        }

        private void OnEditProductClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var product = button?.CommandParameter as Product;

            if (product != null)
            {
                _editingProduct = product;
                NameEntry.Text = product.Name;
                PriceEntry.Text = product.Price.ToString();
            }
        }
    }
}