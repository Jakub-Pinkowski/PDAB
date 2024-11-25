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
    public partial class OrderPage : ContentPage, INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private Order? _editingOrder;
        private string _buttonText = "Add Order";
        private bool _isEditing = false;

        public new event PropertyChangedEventHandler? PropertyChanged;

        public OrderPage(DatabaseService databaseService)
        {
            InitializeComponent();
            BindingContext = this;
            _databaseService = databaseService;
            LoadOrdersAsync();
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

        private async void LoadOrdersAsync()
        {
            var orders = await _databaseService.GetItemsAsync<Order>();
            OrdersCollectionView.ItemsSource = orders;
        }

        private async void OnAddOrderClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CustomerIdEntry.Text) || !int.TryParse(CustomerIdEntry.Text, out var customerId) || customerId <= 0 ||
                string.IsNullOrWhiteSpace(OrderDateEntry.Text) || !DateTime.TryParse(OrderDateEntry.Text, out var orderDate))
            {
                await DisplayAlert("Validation Error", "Please ensure all fields are filled correctly. Customer ID must be a positive integer and Order Date must be a valid date.", "OK");
                return;
            }

            if (_editingOrder == null)
            {
                var newOrder = new Order
                {
                    CustomerId = customerId,
                    OrderDate = orderDate
                };

                await _databaseService.SaveItemAsync(newOrder);
            }
            else
            {
                _editingOrder.CustomerId = customerId;
                _editingOrder.OrderDate = orderDate;
                await _databaseService.SaveItemAsync(_editingOrder);
                _editingOrder = null;
                ButtonText = "Add Order";
                IsEditing = false;
            }

            LoadOrdersAsync();

            // Reset the input fields
            CustomerIdEntry.Text = string.Empty;
            OrderDateEntry.Text = string.Empty;
        }

        private async void OnDeleteOrderClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var order = button?.CommandParameter as Order;

            if (order != null)
            {
                bool confirm = await DisplayAlert("Confirm Delete", $"Are you sure you want to delete the order with ID {order.Id}?", "Yes", "No");
                if (confirm)
                {
                    await _databaseService.DeleteItemAsync(order);
                    LoadOrdersAsync();
                }
            }
        }

        private void OnEditOrderClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var order = button?.CommandParameter as Order;

            if (order != null)
            {
                _editingOrder = order;
                CustomerIdEntry.Text = order.CustomerId.ToString();
                OrderDateEntry.Text = order.OrderDate.ToString("yyyy-MM-dd");
                ButtonText = "Edit Order";
                IsEditing = true;
            }
        }

        private void OnCancelEditClicked(object sender, EventArgs e)
        {
            _editingOrder = null;
            ButtonText = "Add Order";
            IsEditing = false;

            // Reset the input fields
            CustomerIdEntry.Text = string.Empty;
            OrderDateEntry.Text = string.Empty;
        }

        protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}