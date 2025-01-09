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
    public partial class ShoppingCartPage : ContentPage, INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private ShoppingCartItem? _editingCartItem;
        private string _buttonText = "Add Item";
        private bool _isEditing = false;
        private bool _isSortedAscending = true;
        private List<ShoppingCartItem> _masterCartItemList = new List<ShoppingCartItem>();

        public new event PropertyChangedEventHandler? PropertyChanged;

        public ShoppingCartPage(DatabaseService databaseService)
        {
            InitializeComponent();
            BindingContext = this;
            _databaseService = databaseService;
            LoadCartItemsAsync();
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

        private async void LoadCartItemsAsync()
        {
            _masterCartItemList = await _databaseService.GetItemsAsync<ShoppingCartItem>();
            CartItemsCollectionView.ItemsSource = _masterCartItemList;
        }

        private async void OnAddCartItemClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CustomerIdEntry.Text) || !int.TryParse(CustomerIdEntry.Text, out var customerId) || customerId <= 0 ||
                string.IsNullOrWhiteSpace(ProductIdEntry.Text) || !int.TryParse(ProductIdEntry.Text, out var productId) || productId <= 0 ||
                string.IsNullOrWhiteSpace(QuantityEntry.Text) || !int.TryParse(QuantityEntry.Text, out var quantity) || quantity <= 0)
            {
                await DisplayAlert("Validation Error", "Please ensure all fields are filled correctly. Customer ID and Product ID must be positive integers and Quantity must be a positive integer.", "OK");
                return;
            }

            if (_editingCartItem == null)
            {
                var newCartItem = new ShoppingCartItem
                {
                    CustomerId = customerId,
                    ProductId = productId,
                    Quantity = quantity
                };

                await _databaseService.SaveItemAsync(newCartItem);
            }
            else
            {
                _editingCartItem.CustomerId = customerId;
                _editingCartItem.ProductId = productId;
                _editingCartItem.Quantity = quantity;
                await _databaseService.SaveItemAsync(_editingCartItem);
                _editingCartItem = null;
                ButtonText = "Add Item";
                IsEditing = false;
            }

            LoadCartItemsAsync();

            // Reset the input fields
            CustomerIdEntry.Text = string.Empty;
            ProductIdEntry.Text = string.Empty;
            QuantityEntry.Text = string.Empty;
        }

        private async void OnDeleteCartItemClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var cartItem = button?.CommandParameter as ShoppingCartItem;

            if (cartItem != null)
            {
                bool confirm = await DisplayAlert("Confirm Delete", $"Are you sure you want to delete the item with Product ID {cartItem.ProductId}?", "Yes", "No");
                if (confirm)
                {
                    await _databaseService.DeleteItemAsync(cartItem);
                    LoadCartItemsAsync();
                }
            }
        }

        private void OnEditCartItemClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var cartItem = button?.CommandParameter as ShoppingCartItem;

            if (cartItem != null)
            {
                _editingCartItem = cartItem;
                CustomerIdEntry.Text = cartItem.CustomerId.ToString();
                ProductIdEntry.Text = cartItem.ProductId.ToString();
                QuantityEntry.Text = cartItem.Quantity.ToString();
                ButtonText = "Edit Item";
                IsEditing = true;
            }
        }

        private void OnCancelEditClicked(object sender, EventArgs e)
        {
            _editingCartItem = null;
            ButtonText = "Add Item";
            IsEditing = false;

            // Reset the input fields
            CustomerIdEntry.Text = string.Empty;
            ProductIdEntry.Text = string.Empty;
            QuantityEntry.Text = string.Empty;
        }

        private void OnRefreshClicked(object sender, EventArgs e)
        {
            LoadCartItemsAsync();
        }

        private void SortCartItems(string criterion)
        {
            var cartItems = CartItemsCollectionView.ItemsSource.Cast<ShoppingCartItem>().ToList();
            switch (criterion)
            {
                case "CustomerId":
                    cartItems = _isSortedAscending ? cartItems.OrderBy(c => c.CustomerId).ToList() : cartItems.OrderByDescending(c => c.CustomerId).ToList();
                    _isSortedAscending = !_isSortedAscending;
                    break;
                case "ProductId":
                    cartItems = _isSortedAscending ? cartItems.OrderBy(c => c.ProductId).ToList() : cartItems.OrderByDescending(c => c.ProductId).ToList();
                    _isSortedAscending = !_isSortedAscending;
                    break;
                case "Quantity":
                    cartItems = _isSortedAscending ? cartItems.OrderBy(c => c.Quantity).ToList() : cartItems.OrderByDescending(c => c.Quantity).ToList();
                    _isSortedAscending = !_isSortedAscending;
                    break;
            }
            CartItemsCollectionView.ItemsSource = cartItems;
        }

        private void OnSortByCustomerIdClicked(object sender, EventArgs e)
        {
            SortCartItems("CustomerId");
        }

        private void OnSortByProductIdClicked(object sender, EventArgs e)
        {
            SortCartItems("ProductId");
        }

        private void OnSortByQuantityClicked(object sender, EventArgs e)
        {
            SortCartItems("Quantity");
        }

        private void FilterCartItems(string criterion, string minValue, string maxValue)
        {
            var cartItems = _masterCartItemList;
            switch (criterion)
            {
                case "CustomerId":
                    if (int.TryParse(minValue, out int minCustomerId) && int.TryParse(maxValue, out int maxCustomerId))
                    {
                        cartItems = cartItems.Where(c => c.CustomerId >= minCustomerId && c.CustomerId <= maxCustomerId).ToList();
                    }
                    else if (int.TryParse(minValue, out minCustomerId))
                    {
                        cartItems = cartItems.Where(c => c.CustomerId >= minCustomerId).ToList();
                    }
                    else if (int.TryParse(maxValue, out maxCustomerId))
                    {
                        cartItems = cartItems.Where(c => c.CustomerId <= maxCustomerId).ToList();
                    }
                    break;
                case "ProductId":
                    if (int.TryParse(minValue, out int minProductId) && int.TryParse(maxValue, out int maxProductId))
                    {
                        cartItems = cartItems.Where(c => c.ProductId >= minProductId && c.ProductId <= maxProductId).ToList();
                    }
                    else if (int.TryParse(minValue, out minProductId))
                    {
                        cartItems = cartItems.Where(c => c.ProductId >= minProductId).ToList();
                    }
                    else if (int.TryParse(maxValue, out maxProductId))
                    {
                        cartItems = cartItems.Where(c => c.ProductId <= maxProductId).ToList();
                    }
                    break;
                case "Quantity":
                    if (int.TryParse(minValue, out int minQuantity) && int.TryParse(maxValue, out int maxQuantity))
                    {
                        cartItems = cartItems.Where(c => c.Quantity >= minQuantity && c.Quantity <= maxQuantity).ToList();
                    }
                    else if (int.TryParse(minValue, out minQuantity))
                    {
                        cartItems = cartItems.Where(c => c.Quantity >= minQuantity).ToList();
                    }
                    else if (int.TryParse(maxValue, out maxQuantity))
                    {
                        cartItems = cartItems.Where(c => c.Quantity <= maxQuantity).ToList();
                    }
                    break;
            }
            CartItemsCollectionView.ItemsSource = cartItems;
        }

        private void OnFilterByCustomerIdClicked(object sender, EventArgs e)
        {
            FilterCartItems("CustomerId", MinCustomerIdEntry.Text, MaxCustomerIdEntry.Text);
        }

        private void OnFilterByProductIdClicked(object sender, EventArgs e)
        {
            FilterCartItems("ProductId", MinProductIdEntry.Text, MaxProductIdEntry.Text);
        }

        private void OnFilterByQuantityClicked(object sender, EventArgs e)
        {
            FilterCartItems("Quantity", MinQuantityEntry.Text, MaxQuantityEntry.Text);
        }

        private void OnRefreshFiltersClicked(object sender, EventArgs e)
        {
            // Clear all filter inputs
            MinCustomerIdEntry.Text = string.Empty;
            MaxCustomerIdEntry.Text = string.Empty;
            MinProductIdEntry.Text = string.Empty;
            MaxProductIdEntry.Text = string.Empty;
            MinQuantityEntry.Text = string.Empty;
            MaxQuantityEntry.Text = string.Empty;

            // Reset the displayed cart items to the full list
            CartItemsCollectionView.ItemsSource = _masterCartItemList;
        }

        protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}