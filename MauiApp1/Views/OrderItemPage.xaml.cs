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
    public partial class OrderItemPage : ContentPage, INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private OrderItem? _editingOrderItem;
        private string _buttonText = "Add Order Item";
        private bool _isEditing = false;
        private bool _isSortedAscending = true;
        private List<OrderItem> _masterOrderItemList = new List<OrderItem>();

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
            _masterOrderItemList = await _databaseService.GetItemsAsync<OrderItem>();
            OrderItemsCollectionView.ItemsSource = _masterOrderItemList;
        }

        private async void OnAddOrderItemClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(OrderIdEntry.Text) || !int.TryParse(OrderIdEntry.Text, out var orderId) || orderId <= 0 ||
                string.IsNullOrWhiteSpace(ProductIdEntry.Text) || !int.TryParse(ProductIdEntry.Text, out var productId) || productId <= 0 ||
                string.IsNullOrWhiteSpace(QuantityEntry.Text) || !int.TryParse(QuantityEntry.Text, out var quantity) || quantity <= 0 ||
                string.IsNullOrWhiteSpace(PriceEntry.Text) || !decimal.TryParse(PriceEntry.Text, out var Price) || Price <= 0)
            {
                await DisplayAlert("Validation Error", "Please ensure all fields are filled correctly.", "OK");
                return;
            }

            if (_editingOrderItem == null)
            {
                var newOrderItem = new OrderItem
                {
                    OrderId = orderId,
                    ProductId = productId,
                    Quantity = quantity,
                    Price = Price
                };

                await _databaseService.SaveItemAsync(newOrderItem);
            }
            else
            {
                _editingOrderItem.OrderId = orderId;
                _editingOrderItem.ProductId = productId;
                _editingOrderItem.Quantity = quantity;
                _editingOrderItem.Price = Price;
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
                bool confirm = await DisplayAlert("Confirm Delete", $"Are you sure you want to delete the order item with Order ID {orderItem.OrderId}?", "Yes", "No");
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

        private void OnRefreshClicked(object sender, EventArgs e)
        {
            LoadOrderItemsAsync();
        }

        private void SortOrderItems(string criterion)
        {
            var orderItems = OrderItemsCollectionView.ItemsSource.Cast<OrderItem>().ToList();
            switch (criterion)
            {
                case "OrderId":
                    orderItems = _isSortedAscending ? orderItems.OrderBy(oi => oi.OrderId).ToList() : orderItems.OrderByDescending(oi => oi.OrderId).ToList();
                    _isSortedAscending = !_isSortedAscending;
                    break;
                case "ProductId":
                    orderItems = _isSortedAscending ? orderItems.OrderBy(oi => oi.ProductId).ToList() : orderItems.OrderByDescending(oi => oi.ProductId).ToList();
                    _isSortedAscending = !_isSortedAscending;
                    break;
                case "Quantity":
                    orderItems = _isSortedAscending ? orderItems.OrderBy(oi => oi.Quantity).ToList() : orderItems.OrderByDescending(oi => oi.Quantity).ToList();
                    _isSortedAscending = !_isSortedAscending;
                    break;
                case "Price":
                    orderItems = _isSortedAscending ? orderItems.OrderBy(oi => oi.Price).ToList() : orderItems.OrderByDescending(oi => oi.Price).ToList();
                    _isSortedAscending = !_isSortedAscending;
                    break;
            }
            OrderItemsCollectionView.ItemsSource = orderItems;
        }

        private void OnSortByOrderIdClicked(object sender, EventArgs e)
        {
            SortOrderItems("OrderId");
        }

        private void OnSortByProductIdClicked(object sender, EventArgs e)
        {
            SortOrderItems("ProductId");
        }

        private void OnSortByQuantityClicked(object sender, EventArgs e)
        {
            SortOrderItems("Quantity");
        }

        private void OnSortByPriceClicked(object sender, EventArgs e)
        {
            SortOrderItems("Price");
        }

        private void FilterOrderItems(string criterion, string minValue, string maxValue)
        {
            var orderItems = _masterOrderItemList;
            switch (criterion)
            {
                case "OrderId":
                    if (int.TryParse(minValue, out int minOrderId) && int.TryParse(maxValue, out int maxOrderId))
                    {
                        orderItems = orderItems.Where(oi => oi.OrderId >= minOrderId && oi.OrderId <= maxOrderId).ToList();
                    }
                    else if (int.TryParse(minValue, out minOrderId))
                    {
                        orderItems = orderItems.Where(oi => oi.OrderId >= minOrderId).ToList();
                    }
                    else if (int.TryParse(maxValue, out maxOrderId))
                    {
                        orderItems = orderItems.Where(oi => oi.OrderId <= maxOrderId).ToList();
                    }
                    break;
                case "ProductId":
                    if (int.TryParse(minValue, out int minProductId) && int.TryParse(maxValue, out int maxProductId))
                    {
                        orderItems = orderItems.Where(oi => oi.ProductId >= minProductId && oi.ProductId <= maxProductId).ToList();
                    }
                    else if (int.TryParse(minValue, out minProductId))
                    {
                        orderItems = orderItems.Where(oi => oi.ProductId >= minProductId).ToList();
                    }
                    else if (int.TryParse(maxValue, out maxProductId))
                    {
                        orderItems = orderItems.Where(oi => oi.ProductId <= maxProductId).ToList();
                    }
                    break;
                case "Quantity":
                    if (int.TryParse(minValue, out int minQuantity) && int.TryParse(maxValue, out int maxQuantity))
                    {
                        orderItems = orderItems.Where(oi => oi.Quantity >= minQuantity && oi.Quantity <= maxQuantity).ToList();
                    }
                    else if (int.TryParse(minValue, out minQuantity))
                    {
                        orderItems = orderItems.Where(oi => oi.Quantity >= minQuantity).ToList();
                    }
                    else if (int.TryParse(maxValue, out maxQuantity))
                    {
                        orderItems = orderItems.Where(oi => oi.Quantity <= maxQuantity).ToList();
                    }
                    break;
                case "Price":
                    if (decimal.TryParse(minValue, out decimal minPrice) && decimal.TryParse(maxValue, out decimal maxPrice))
                    {
                        orderItems = orderItems.Where(oi => oi.Price >= minPrice && oi.Price <= maxPrice).ToList();
                    }
                    else if (decimal.TryParse(minValue, out minPrice))
                    {
                        orderItems = orderItems.Where(oi => oi.Price >= minPrice).ToList();
                    }
                    else if (decimal.TryParse(maxValue, out maxPrice))
                    {
                        orderItems = orderItems.Where(oi => oi.Price <= maxPrice).ToList();
                    }
                    break;
            }
            OrderItemsCollectionView.ItemsSource = orderItems;
        }

        private void OnFilterByOrderIdClicked(object sender, EventArgs e)
        {
            FilterOrderItems("OrderId", MinOrderIdEntry.Text, MaxOrderIdEntry.Text);
        }

        private void OnFilterByProductIdClicked(object sender, EventArgs e)
        {
            FilterOrderItems("ProductId", MinProductIdEntry.Text, MaxProductIdEntry.Text);
        }

        private void OnFilterByQuantityClicked(object sender, EventArgs e)
        {
            FilterOrderItems("Quantity", MinQuantityEntry.Text, MaxQuantityEntry.Text);
        }

        private void OnFilterByPriceClicked(object sender, EventArgs e)
        {
            FilterOrderItems("Price", MinPriceEntry.Text, MaxPriceEntry.Text);
        }

        private void OnRefreshFiltersClicked(object sender, EventArgs e)
        {
            // Clear all filter inputs
            MinOrderIdEntry.Text = string.Empty;
            MaxOrderIdEntry.Text = string.Empty;
            MinProductIdEntry.Text = string.Empty;
            MaxProductIdEntry.Text = string.Empty;
            MinQuantityEntry.Text = string.Empty;
            MaxQuantityEntry.Text = string.Empty;
            MinPriceEntry.Text = string.Empty;
            MaxPriceEntry.Text = string.Empty;

            // Reset the displayed order items to the full list
            OrderItemsCollectionView.ItemsSource = _masterOrderItemList;
        }

        protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}