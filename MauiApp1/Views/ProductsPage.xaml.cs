using MauiApp1.Models;
using MauiApp1.Services;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MauiApp1
{
    public partial class ProductsPage : ContentPage, INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private Product? _editingProduct;
        private string _buttonText = "Add Product";
        private bool _isEditing = false;
        private bool _isSortedAscending = true;
        private List<Product> _masterProductList = new List<Product>();

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

        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                _isEditing = value;
                OnPropertyChanged();
            }
        }

        private async void LoadProductsAsync()
        {
            _masterProductList = await _databaseService.GetItemsAsync<Product>();
            ProductsCollectionView.ItemsSource = _masterProductList;
        }

        private async void OnAddProductClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameEntry.Text) || NameEntry.Text.Length < 2 ||
                string.IsNullOrWhiteSpace(PriceEntry.Text) || !decimal.TryParse(PriceEntry.Text, out var price) || price <= 0 ||
                string.IsNullOrWhiteSpace(CategoryIdEntry.Text) || !int.TryParse(CategoryIdEntry.Text, out var categoryId) || categoryId <= 0 ||
                string.IsNullOrWhiteSpace(SupplierIdEntry.Text) || !int.TryParse(SupplierIdEntry.Text, out var supplierId) || supplierId <= 0)
            {
                await DisplayAlert("Validation Error", "Please ensure all fields are filled correctly.", "OK");
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
                IsEditing = false;
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
                bool confirm = await DisplayAlert("Confirm Delete", $"Are you sure you want to delete the product {product.Name}?", "Yes", "No");
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
                IsEditing = true;
            }
        }

        private void OnCancelEditClicked(object sender, EventArgs e)
        {
            _editingProduct = null;
            ButtonText = "Add Product";
            IsEditing = false;

            // Reset the input fields
            NameEntry.Text = string.Empty;
            PriceEntry.Text = string.Empty;
            CategoryIdEntry.Text = string.Empty;
            SupplierIdEntry.Text = string.Empty;
        }

        private void OnRefreshClicked(object sender, EventArgs e)
        {
            LoadProductsAsync();
        }

        private void SortProducts(string criterion)
        {
            var products = ProductsCollectionView.ItemsSource.Cast<Product>().ToList();
            switch (criterion)
            {
                case "Name":
                    products = _isSortedAscending ? products.OrderBy(p => p.Name).ToList() : products.OrderByDescending(p => p.Name).ToList();
                    _isSortedAscending = !_isSortedAscending;
                    break;
                case "Price":
                    products = _isSortedAscending ? products.OrderBy(p => p.Price).ToList() : products.OrderByDescending(p => p.Price).ToList();
                    _isSortedAscending = !_isSortedAscending;
                    break;
                case "CategoryId":
                    products = _isSortedAscending ? products.OrderBy(p => p.CategoryId).ToList() : products.OrderByDescending(p => p.CategoryId).ToList();
                    _isSortedAscending = !_isSortedAscending;
                    break;
                case "SupplierId":
                    products = _isSortedAscending ? products.OrderBy(p => p.SupplierId).ToList() : products.OrderByDescending(p => p.SupplierId).ToList();
                    _isSortedAscending = !_isSortedAscending;
                    break;
            }
            ProductsCollectionView.ItemsSource = products;
        }

        private void OnSortByNameClicked(object sender, EventArgs e)
        {
            SortProducts("Name");
        }

        private void OnSortByPriceClicked(object sender, EventArgs e)
        {
            SortProducts("Price");
        }

        private void OnSortByCategoryIdClicked(object sender, EventArgs e)
        {
            SortProducts("CategoryId");
        }

        private void OnSortBySupplierIdClicked(object sender, EventArgs e)
        {
            SortProducts("SupplierId");
        }

        private void FilterProducts(string criterion, string minValue, string maxValue)
        {
            var products = _masterProductList;
            switch (criterion)
            {
                case "Name":
                    if (!string.IsNullOrWhiteSpace(minValue))
                    {
                        products = products.Where(p => p.Name.Contains(minValue)).ToList();
                    }
                    break;
                case "Price":
                    if (decimal.TryParse(minValue, out decimal minPrice) && decimal.TryParse(maxValue, out decimal maxPrice))
                    {
                        products = products.Where(p => p.Price >= minPrice && p.Price <= maxPrice).ToList();
                    }
                    else if (decimal.TryParse(minValue, out minPrice))
                    {
                        products = products.Where(p => p.Price >= minPrice).ToList();
                    }
                    else if (decimal.TryParse(maxValue, out maxPrice))
                    {
                        products = products.Where(p => p.Price <= maxPrice).ToList();
                    }
                    break;
                case "CategoryId":
                    if (int.TryParse(minValue, out int minCategoryId) && int.TryParse(maxValue, out int maxCategoryId))
                    {
                        products = products.Where(p => p.CategoryId >= minCategoryId && p.CategoryId <= maxCategoryId).ToList();
                    }
                    else if (int.TryParse(minValue, out minCategoryId))
                    {
                        products = products.Where(p => p.CategoryId >= minCategoryId).ToList();
                    }
                    else if (int.TryParse(maxValue, out maxCategoryId))
                    {
                        products = products.Where(p => p.CategoryId <= maxCategoryId).ToList();
                    }
                    break;
                case "SupplierId":
                    if (int.TryParse(minValue, out int minSupplierId) && int.TryParse(maxValue, out int maxSupplierId))
                    {
                        products = products.Where(p => p.SupplierId >= minSupplierId && p.SupplierId <= maxSupplierId).ToList();
                    }
                    else if (int.TryParse(minValue, out minSupplierId))
                    {
                        products = products.Where(p => p.SupplierId >= minSupplierId).ToList();
                    }
                    else if (int.TryParse(maxValue, out maxSupplierId))
                    {
                        products = products.Where(p => p.SupplierId <= maxSupplierId).ToList();
                    }
                    break;
            }
            ProductsCollectionView.ItemsSource = products;
        }

        private void OnFilterByNameClicked(object sender, EventArgs e)
        {
            FilterProducts("Name", MinNameEntry.Text, MaxNameEntry.Text);
        }

        private void OnFilterByPriceClicked(object sender, EventArgs e)
        {
            FilterProducts("Price", MinPriceEntry.Text, MaxPriceEntry.Text);
        }

        private void OnFilterByCategoryIdClicked(object sender, EventArgs e)
        {
            FilterProducts("CategoryId", MinCategoryIdEntry.Text, MaxCategoryIdEntry.Text);
        }

        private void OnFilterBySupplierIdClicked(object sender, EventArgs e)
        {
            FilterProducts("SupplierId", MinSupplierIdEntry.Text, MaxSupplierIdEntry.Text);
        }

        private void OnRefreshFiltersClicked(object sender, EventArgs e)
        {
            // Clear all filter inputs
            MinNameEntry.Text = string.Empty;
            MaxNameEntry.Text = string.Empty;
            MinPriceEntry.Text = string.Empty;
            MaxPriceEntry.Text = string.Empty;
            MinCategoryIdEntry.Text = string.Empty;
            MaxCategoryIdEntry.Text = string.Empty;
            MinSupplierIdEntry.Text = string.Empty;
            MaxSupplierIdEntry.Text = string.Empty;

            // Reset the displayed products to the full list
            ProductsCollectionView.ItemsSource = _masterProductList;
        }

        protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}