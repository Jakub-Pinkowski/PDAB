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
    public partial class ShoppingCartPage : ContentPage, INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private ShoppingCartItem? _editingCartItem;
        private string _buttonText = "Add Item";
        private bool _isEditing = false;

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
            var cartItems = await _databaseService.GetItemsAsync<ShoppingCartItem>();
            CartItemsCollectionView.ItemsSource = cartItems;
        }

        private async void OnAddCartItemClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ProductIdEntry.Text) || !int.TryParse(ProductIdEntry.Text, out var productId) || productId <= 0 ||
                string.IsNullOrWhiteSpace(QuantityEntry.Text) || !int.TryParse(QuantityEntry.Text, out var quantity) || quantity <= 0)
            {
                await DisplayAlert("Validation Error", "Please ensure all fields are filled correctly. Product ID must be a positive integer and Quantity must be a positive integer.", "OK");
                return;
            }

            if (_editingCartItem == null)
            {
                var newCartItem = new ShoppingCartItem
                {
                    ProductId = productId,
                    Quantity = quantity
                };

                await _databaseService.SaveItemAsync(newCartItem);
            }
            else
            {
                _editingCartItem.ProductId = productId;
                _editingCartItem.Quantity = quantity;
                await _databaseService.SaveItemAsync(_editingCartItem);
                _editingCartItem = null;
                ButtonText = "Add Item";
                IsEditing = false;
            }

            LoadCartItemsAsync();

            // Reset the input fields
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
            ProductIdEntry.Text = string.Empty;
            QuantityEntry.Text = string.Empty;
        }

        private void OnRefreshClicked(object sender, EventArgs e)
        {
            LoadCartItemsAsync();
        }

        protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}