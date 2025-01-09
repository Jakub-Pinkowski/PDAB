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
    public partial class OrderPage : ContentPage, INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private Order? _editingOrder;
        private string _buttonText = "Add Order";
        private bool _isEditing = false;
        private bool _isSortedAscending = true;
        private List<Order> _masterOrderList = new List<Order>();

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
            _masterOrderList = await _databaseService.GetItemsAsync<Order>();
            OrdersCollectionView.ItemsSource = _masterOrderList;
        }

        private async void OnAddOrderClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CustomerIdEntry.Text) || !int.TryParse(CustomerIdEntry.Text, out var customerId) || customerId <= 0 ||
                string.IsNullOrWhiteSpace(OrderDateEntry.Text) || !DateTime.TryParse(OrderDateEntry.Text, out var orderDate))
            {
                await DisplayAlert("Validation Error", "Please ensure all fields are filled correctly. Customer ID must be a positive integer, and Order Date must be a valid date.", "OK");
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

        private void OnRefreshClicked(object sender, EventArgs e)
        {
            LoadOrdersAsync();
        }

        private void SortOrders(string criterion)
        {
            var orders = OrdersCollectionView.ItemsSource.Cast<Order>().ToList();
            switch (criterion)
            {
                case "CustomerId":
                    orders = _isSortedAscending ? orders.OrderBy(o => o.CustomerId).ToList() : orders.OrderByDescending(o => o.CustomerId).ToList();
                    _isSortedAscending = !_isSortedAscending;
                    break;
                case "OrderDate":
                    orders = _isSortedAscending ? orders.OrderBy(o => o.OrderDate).ToList() : orders.OrderByDescending(o => o.OrderDate).ToList();
                    _isSortedAscending = !_isSortedAscending;
                    break;
            }
            OrdersCollectionView.ItemsSource = orders;
        }

        private void OnSortByCustomerIdClicked(object sender, EventArgs e)
        {
            SortOrders("CustomerId");
        }

        private void OnSortByOrderDateClicked(object sender, EventArgs e)
        {
            SortOrders("OrderDate");
        }

        private void FilterOrders(string criterion, string minValue, string maxValue)
        {
            var orders = _masterOrderList;
            switch (criterion)
            {
                case "CustomerId":
                    if (int.TryParse(minValue, out int minCustomerId) && int.TryParse(maxValue, out int maxCustomerId))
                    {
                        orders = orders.Where(o => o.CustomerId >= minCustomerId && o.CustomerId <= maxCustomerId).ToList();
                    }
                    else if (int.TryParse(minValue, out minCustomerId))
                    {
                        orders = orders.Where(o => o.CustomerId >= minCustomerId).ToList();
                    }
                    else if (int.TryParse(maxValue, out maxCustomerId))
                    {
                        orders = orders.Where(o => o.CustomerId <= maxCustomerId).ToList();
                    }
                    break;
                case "OrderDate":
                    if (DateTime.TryParse(minValue, out DateTime minOrderDate) && DateTime.TryParse(maxValue, out DateTime maxOrderDate))
                    {
                        orders = orders.Where(o => o.OrderDate.Date >= minOrderDate.Date && o.OrderDate.Date <= maxOrderDate.Date).ToList();
                    }
                    else if (DateTime.TryParse(minValue, out minOrderDate))
                    {
                        orders = orders.Where(o => o.OrderDate.Date >= minOrderDate.Date).ToList();
                    }
                    else if (DateTime.TryParse(maxValue, out maxOrderDate))
                    {
                        orders = orders.Where(o => o.OrderDate.Date <= maxOrderDate.Date).ToList();
                    }
                    break;
            }
            OrdersCollectionView.ItemsSource = orders;
        }

        private void OnFilterByCustomerIdClicked(object sender, EventArgs e)
        {
            FilterOrders("CustomerId", MinCustomerIdEntry.Text, MaxCustomerIdEntry.Text);
        }

        private void OnFilterByOrderDateClicked(object sender, EventArgs e)
        {
            FilterOrders("OrderDate", MinOrderDateEntry.Text, MaxOrderDateEntry.Text);
        }

        private void OnRefreshFiltersClicked(object sender, EventArgs e)
        {
            // Clear all filter inputs
            MinCustomerIdEntry.Text = string.Empty;
            MaxCustomerIdEntry.Text = string.Empty;
            MinOrderDateEntry.Text = string.Empty;
            MaxOrderDateEntry.Text = string.Empty;

            // Reset the displayed orders to the full list
            OrdersCollectionView.ItemsSource = _masterOrderList;
        }

        protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}