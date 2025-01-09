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
    public partial class WishlistPage : ContentPage, INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private WishlistItem? _editingWishlistItem;
        private string _buttonText = "Add Item";
        private bool _isEditing = false;
        private bool _isSortedAscending = true;
        private List<WishlistItem> _masterWishlistItemList = new List<WishlistItem>();

        public new event PropertyChangedEventHandler? PropertyChanged;

        public WishlistPage(DatabaseService databaseService)
        {
            InitializeComponent();
            BindingContext = this;
            _databaseService = databaseService;
            LoadWishlistItemsAsync();
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

        private async void LoadWishlistItemsAsync()
        {
            _masterWishlistItemList = await _databaseService.GetItemsAsync<WishlistItem>();
            WishlistItemsCollectionView.ItemsSource = _masterWishlistItemList;
        }

        private async void OnAddWishlistItemClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ProductIdEntry.Text) || !int.TryParse(ProductIdEntry.Text, out var productId) || productId <= 0 ||
                string.IsNullOrWhiteSpace(CustomerIdEntry.Text) || !int.TryParse(CustomerIdEntry.Text, out var customerId) || customerId <= 0)
            {
                await DisplayAlert("Validation Error", "Please ensure all fields are filled correctly. Product ID and Customer ID must be positive integers.", "OK");
                return;
            }

            if (_editingWishlistItem == null)
            {
                var newWishlistItem = new WishlistItem
                {
                    ProductId = productId,
                    CustomerId = customerId
                };

                await _databaseService.SaveItemAsync(newWishlistItem);
            }
            else
            {
                _editingWishlistItem.ProductId = productId;
                _editingWishlistItem.CustomerId = customerId;
                await _databaseService.SaveItemAsync(_editingWishlistItem);
                _editingWishlistItem = null;
                ButtonText = "Add Item";
                IsEditing = false;
            }

            LoadWishlistItemsAsync();

            // Reset the input fields
            ProductIdEntry.Text = string.Empty;
            CustomerIdEntry.Text = string.Empty;
        }

        private async void OnDeleteWishlistItemClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var wishlistItem = button?.CommandParameter as WishlistItem;

            if (wishlistItem != null)
            {
                bool confirm = await DisplayAlert("Confirm Delete", $"Are you sure you want to delete the item with Product ID {wishlistItem.ProductId}?", "Yes", "No");
                if (confirm)
                {
                    await _databaseService.DeleteItemAsync(wishlistItem);
                    LoadWishlistItemsAsync();
                }
            }
        }

        private void OnEditWishlistItemClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var wishlistItem = button?.CommandParameter as WishlistItem;

            if (wishlistItem != null)
            {
                _editingWishlistItem = wishlistItem;
                ProductIdEntry.Text = wishlistItem.ProductId.ToString();
                CustomerIdEntry.Text = wishlistItem.CustomerId.ToString();
                ButtonText = "Edit Item";
                IsEditing = true;
            }
        }

        private void OnCancelEditClicked(object sender, EventArgs e)
        {
            _editingWishlistItem = null;
            ButtonText = "Add Item";
            IsEditing = false;

            // Reset the input fields
            ProductIdEntry.Text = string.Empty;
            CustomerIdEntry.Text = string.Empty;
        }

        private void OnRefreshClicked(object sender, EventArgs e)
        {
            LoadWishlistItemsAsync();
        }

        private void SortWishlistItems(string criterion)
        {
            var wishlistItems = WishlistItemsCollectionView.ItemsSource.Cast<WishlistItem>().ToList();
            switch (criterion)
            {
                case "ProductId":
                    wishlistItems = _isSortedAscending ? wishlistItems.OrderBy(w => w.ProductId).ToList() : wishlistItems.OrderByDescending(w => w.ProductId).ToList();
                    _isSortedAscending = !_isSortedAscending;
                    break;
                case "CustomerId":
                    wishlistItems = _isSortedAscending ? wishlistItems.OrderBy(w => w.CustomerId).ToList() : wishlistItems.OrderByDescending(w => w.CustomerId).ToList();
                    _isSortedAscending = !_isSortedAscending;
                    break;
            }
            WishlistItemsCollectionView.ItemsSource = wishlistItems;
        }

        private void OnSortByProductIdClicked(object sender, EventArgs e)
        {
            SortWishlistItems("ProductId");
        }

        private void OnSortByCustomerIdClicked(object sender, EventArgs e)
        {
            SortWishlistItems("CustomerId");
        }

        private void FilterWishlistItems(string criterion, string minValue, string maxValue)
        {
            var wishlistItems = _masterWishlistItemList;
            switch (criterion)
            {
                case "ProductId":
                    if (int.TryParse(minValue, out int minProductId) && int.TryParse(maxValue, out int maxProductId))
                    {
                        wishlistItems = wishlistItems.Where(w => w.ProductId >= minProductId && w.ProductId <= maxProductId).ToList();
                    }
                    else if (int.TryParse(minValue, out minProductId))
                    {
                        wishlistItems = wishlistItems.Where(w => w.ProductId >= minProductId).ToList();
                    }
                    else if (int.TryParse(maxValue, out maxProductId))
                    {
                        wishlistItems = wishlistItems.Where(w => w.ProductId <= maxProductId).ToList();
                    }
                    break;
                case "CustomerId":
                    if (int.TryParse(minValue, out int minCustomerId) && int.TryParse(maxValue, out int maxCustomerId))
                    {
                        wishlistItems = wishlistItems.Where(w => w.CustomerId >= minCustomerId && w.CustomerId <= maxCustomerId).ToList();
                    }
                    else if (int.TryParse(minValue, out minCustomerId))
                    {
                        wishlistItems = wishlistItems.Where(w => w.CustomerId >= minCustomerId).ToList();
                    }
                    else if (int.TryParse(maxValue, out maxCustomerId))
                    {
                        wishlistItems = wishlistItems.Where(w => w.CustomerId <= maxCustomerId).ToList();
                    }
                    break;
            }
            WishlistItemsCollectionView.ItemsSource = wishlistItems;
        }

        private void OnFilterByProductIdClicked(object sender, EventArgs e)
        {
            FilterWishlistItems("ProductId", MinProductIdEntry.Text, MaxProductIdEntry.Text);
        }

        private void OnFilterByCustomerIdClicked(object sender, EventArgs e)
        {
            FilterWishlistItems("CustomerId", MinCustomerIdEntry.Text, MaxCustomerIdEntry.Text);
        }

        private void OnRefreshFiltersClicked(object sender, EventArgs e)
        {
            // Clear all filter inputs
            MinProductIdEntry.Text = string.Empty;
            MaxProductIdEntry.Text = string.Empty;
            MinCustomerIdEntry.Text = string.Empty;
            MaxCustomerIdEntry.Text = string.Empty;

            // Reset the displayed wishlist items to the full list
            WishlistItemsCollectionView.ItemsSource = _masterWishlistItemList;
        }

        protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}