using MauiApp1.Models;
using MauiApp1.Services;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MauiApp1
{
    public partial class ProductsPage : ContentPage, INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private Product? _editingProduct;
        private string _buttonText = "Add Product";

        public new event PropertyChangedEventHandler? PropertyChanged;

        public ProductsPage(DatabaseService databaseService)
        {
            InitializeComponent();
            BindingContext = this;
            _databaseService = databaseService;
            LoadProductsAsync();
        }

        public string ButtonText
        {
            get => _buttonText;
            set
            {
                _buttonText = value;
                OnPropertyChanged();
            }
        }

        private async void LoadProductsAsync()
        {
            var products = await _databaseService.GetItemsAsync<Product>();
            ProductsCollectionView.ItemsSource = products;
        }

        private async void OnAddProductClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameEntry.Text) || NameEntry.Text.Length < 2 ||
                string.IsNullOrWhiteSpace(PriceEntry.Text) || !decimal.TryParse(PriceEntry.Text, out var price) || price <= 0 ||
                string.IsNullOrWhiteSpace(CategoryIdEntry.Text) || !int.TryParse(CategoryIdEntry.Text, out var categoryId) || categoryId <= 0 ||
                string.IsNullOrWhiteSpace(SupplierIdEntry.Text) || !int.TryParse(SupplierIdEntry.Text, out var supplierId) || supplierId <= 0)
            {
                await DisplayAlert("Validation Error", "Please ensure all fields are filled correctly. Name must be at least 2 characters long, Price must be a positive number, and CategoryId and SupplierId must be valid positive integers.", "OK");
                return;
            }

            if (_editingProduct == null)
            {
                var newProduct = new Product
                {
                    Name = NameEntry.Text,
                    Price = price,
                    CategoryId = categoryId,
                    SupplierId = supplierId
                };

                await _databaseService.SaveItemAsync(newProduct);
            }
            else
            {
                _editingProduct.Name = NameEntry.Text;
                _editingProduct.Price = price;
                _editingProduct.CategoryId = categoryId;
                _editingProduct.SupplierId = supplierId;
                await _databaseService.SaveItemAsync(_editingProduct);
                _editingProduct = null;
                ButtonText = "Add Product";
            }

            LoadProductsAsync();

            // Reset the input fields
            NameEntry.Text = string.Empty;
            PriceEntry.Text = string.Empty;
            CategoryIdEntry.Text = string.Empty;
            SupplierIdEntry.Text = string.Empty;
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
                CategoryIdEntry.Text = product.CategoryId.ToString();
                SupplierIdEntry.Text = product.SupplierId.ToString();
                ButtonText = "Edit Product";
            }
        }

        protected new void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}