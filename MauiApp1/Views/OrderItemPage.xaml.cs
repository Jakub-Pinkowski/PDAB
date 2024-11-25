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
    public partial class OrderItemPage : ContentPage, INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private OrderItem? _editingOrderItem;
        private string _buttonText = "Add Order Item";
        private bool _isEditing = false;

        public new event PropertyChangedEventHandler? PropertyChanged;

        public OrderItemPage(DatabaseService databaseService)
        {
            InitializeComponent();
            BindingContext = this;
            _databaseService = databaseService;
            LoadOrderItemsAsync();
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

        private async void LoadOrderItemsAsync()
        {
            var orderItems = await _databaseService.GetItemsAsync<OrderItem>();
            OrderItemsCollectionView.ItemsSource = orderItems;
        }

        private async void OnAddOrderItemClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(OrderIdEntry.Text) || !int.TryParse(OrderIdEntry.Text, out var orderId) || orderId <= 0 ||
                string.IsNullOrWhiteSpace(ProductIdEntry.Text) || !int.TryParse(ProductIdEntry.Text, out var productId) || productId <= 0 ||
                string.IsNullOrWhiteSpace(QuantityEntry.Text) || !int.TryParse(QuantityEntry.Text, out var quantity) || quantity <= 0 ||
                string.IsNullOrWhiteSpace(PriceEntry.Text) || !decimal.TryParse(PriceEntry.Text, out var price) || price <= 0)
            {
                await DisplayAlert("Validation Error", "Please ensure all fields are filled correctly. Order ID and Product ID must be positive integers, Quantity must be a positive integer, and Price must be a positive number.", "OK");
                return;
            }

            if (_editingOrderItem == null)
            {
                var newOrderItem = new OrderItem
                {
                    OrderId = orderId,
                    ProductId = productId,
                    Quantity = quantity,
                    Price = price
                };

                await _databaseService.SaveItemAsync(newOrderItem);
            }
            else
            {
                _editingOrderItem.OrderId = orderId;
                _editingOrderItem.ProductId = productId;
                _editingOrderItem.Quantity = quantity;
                _editingOrderItem.Price = price;
                await _databaseService.SaveItemAsync(_editingOrderItem);
                _editingOrderItem = null;
                ButtonText = "Add Order Item";
                IsEditing = false;
            }

            LoadOrderItemsAsync();

            // Reset the input fields
            OrderIdEntry.Text = string.Empty;
            ProductIdEntry.Text = string.Empty;
            QuantityEntry.Text = string.Empty;
            PriceEntry.Text = string.Empty;
        }

        private async void OnDeleteOrderItemClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var orderItem = button?.CommandParameter as OrderItem;

            if (orderItem != null)
            {
                bool confirm = await DisplayAlert("Confirm Delete", $"Are you sure you want to delete the order item with ID {orderItem.Id}?", "Yes", "No");
                if (confirm)
                {
                    await _databaseService.DeleteItemAsync(orderItem);
                    LoadOrderItemsAsync();
                }
            }
        }

        private void OnEditOrderItemClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var orderItem = button?.CommandParameter as OrderItem;

            if (orderItem != null)
            {
                _editingOrderItem = orderItem;
                OrderIdEntry.Text = orderItem.OrderId.ToString();
                ProductIdEntry.Text = orderItem.ProductId.ToString();
                QuantityEntry.Text = orderItem.Quantity.ToString();
                PriceEntry.Text = orderItem.Price.ToString();
                ButtonText = "Edit Order Item";
                IsEditing = true;
            }
        }

        private void OnCancelEditClicked(object sender, EventArgs e)
        {
            _editingOrderItem = null;
            ButtonText = "Add Order Item";
            IsEditing = false;

            // Reset the input fields
            OrderIdEntry.Text = string.Empty;
            ProductIdEntry.Text = string.Empty;
            QuantityEntry.Text = string.Empty;
            PriceEntry.Text = string.Empty;
        }

        protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}