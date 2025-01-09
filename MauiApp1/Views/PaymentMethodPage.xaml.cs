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
    public partial class PaymentMethodPage : ContentPage, INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private PaymentMethod? _editingPaymentMethod;
        private string _buttonText = "Add Payment Method";
        private bool _isEditing = false;
        private bool _isSortedAscending = true;
        private List<PaymentMethod> _masterPaymentMethodList = new List<PaymentMethod>();

        public new event PropertyChangedEventHandler? PropertyChanged;

        public PaymentMethodPage(DatabaseService databaseService)
        {
            InitializeComponent();
            BindingContext = this;
            _databaseService = databaseService;
            LoadPaymentMethodsAsync();
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

        private async void LoadPaymentMethodsAsync()
        {
            _masterPaymentMethodList = await _databaseService.GetItemsAsync<PaymentMethod>();
            PaymentMethodsCollectionView.ItemsSource = _masterPaymentMethodList;
        }

        private async void OnAddPaymentMethodClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CustomerIdEntry.Text) || !int.TryParse(CustomerIdEntry.Text, out var customerId) || customerId <= 0 ||
                string.IsNullOrWhiteSpace(CardNumberEntry.Text) || CardNumberEntry.Text.Length < 2 ||
                string.IsNullOrWhiteSpace(ExpirationDateEntry.Text) || !DateTime.TryParse(ExpirationDateEntry.Text, out var expirationDate))
            {
                await DisplayAlert("Validation Error", "Please ensure all fields are filled correctly.", "OK");
                return;
            }

            if (_editingPaymentMethod == null)
            {
                var newPaymentMethod = new PaymentMethod
                {
                    CustomerId = customerId,
                    CardNumber = CardNumberEntry.Text,
                    ExpirationDate = expirationDate
                };

                await _databaseService.SaveItemAsync(newPaymentMethod);
            }
            else
            {
                _editingPaymentMethod.CustomerId = customerId;
                _editingPaymentMethod.CardNumber = CardNumberEntry.Text;
                _editingPaymentMethod.ExpirationDate = expirationDate;
                await _databaseService.SaveItemAsync(_editingPaymentMethod);
                _editingPaymentMethod = null;
                ButtonText = "Add Payment Method";
                IsEditing = false;
            }

            LoadPaymentMethodsAsync();

            // Reset the input fields
            CustomerIdEntry.Text = string.Empty;
            CardNumberEntry.Text = string.Empty;
            ExpirationDateEntry.Text = string.Empty;
        }

        private async void OnDeletePaymentMethodClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var paymentMethod = button?.CommandParameter as PaymentMethod;

            if (paymentMethod != null)
            {
                bool confirm = await DisplayAlert("Confirm Delete", $"Are you sure you want to delete the payment method with Card Number {paymentMethod.CardNumber}?", "Yes", "No");
                if (confirm)
                {
                    await _databaseService.DeleteItemAsync(paymentMethod);
                    LoadPaymentMethodsAsync();
                }
            }
        }

        private void OnEditPaymentMethodClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var paymentMethod = button?.CommandParameter as PaymentMethod;

            if (paymentMethod != null)
            {
                _editingPaymentMethod = paymentMethod;
                CustomerIdEntry.Text = paymentMethod.CustomerId.ToString();
                CardNumberEntry.Text = paymentMethod.CardNumber;
                ExpirationDateEntry.Text = paymentMethod.ExpirationDate.ToString("yyyy-MM-dd");
                ButtonText = "Edit Payment Method";
                IsEditing = true;
            }
        }

        private void OnCancelEditClicked(object sender, EventArgs e)
        {
            _editingPaymentMethod = null;
            ButtonText = "Add Payment Method";
            IsEditing = false;

            // Reset the input fields
            CustomerIdEntry.Text = string.Empty;
            CardNumberEntry.Text = string.Empty;
            ExpirationDateEntry.Text = string.Empty;
        }

        private void OnRefreshClicked(object sender, EventArgs e)
        {
            LoadPaymentMethodsAsync();
        }

        private void SortPaymentMethods(string criterion)
        {
            var paymentMethods = PaymentMethodsCollectionView.ItemsSource.Cast<PaymentMethod>().ToList();
            switch (criterion)
            {
                case "CustomerId":
                    paymentMethods = _isSortedAscending ? paymentMethods.OrderBy(p => p.CustomerId).ToList() : paymentMethods.OrderByDescending(p => p.CustomerId).ToList();
                    _isSortedAscending = !_isSortedAscending;
                    break;
                case "CardNumber":
                    paymentMethods = _isSortedAscending ? paymentMethods.OrderBy(p => p.CardNumber).ToList() : paymentMethods.OrderByDescending(p => p.CardNumber).ToList();
                    _isSortedAscending = !_isSortedAscending;
                    break;
                case "ExpirationDate":
                    paymentMethods = _isSortedAscending ? paymentMethods.OrderBy(p => p.ExpirationDate).ToList() : paymentMethods.OrderByDescending(p => p.ExpirationDate).ToList();
                    _isSortedAscending = !_isSortedAscending;
                    break;
            }
            PaymentMethodsCollectionView.ItemsSource = paymentMethods;
        }

        private void OnSortByCustomerIdClicked(object sender, EventArgs e)
        {
            SortPaymentMethods("CustomerId");
        }

        private void OnSortByCardNumberClicked(object sender, EventArgs e)
        {
            SortPaymentMethods("CardNumber");
        }

        private void OnSortByExpirationDateClicked(object sender, EventArgs e)
        {
            SortPaymentMethods("ExpirationDate");
        }

        private void FilterPaymentMethods(string criterion, string minValue, string maxValue)
        {
            var paymentMethods = _masterPaymentMethodList;
            switch (criterion)
            {
                case "CustomerId":
                    if (int.TryParse(minValue, out int minCustomerId) && int.TryParse(maxValue, out int maxCustomerId))
                    {
                        paymentMethods = paymentMethods.Where(p => p.CustomerId >= minCustomerId && p.CustomerId <= maxCustomerId).ToList();
                    }
                    else if (int.TryParse(minValue, out minCustomerId))
                    {
                        paymentMethods = paymentMethods.Where(p => p.CustomerId >= minCustomerId).ToList();
                    }
                    else if (int.TryParse(maxValue, out maxCustomerId))
                    {
                        paymentMethods = paymentMethods.Where(p => p.CustomerId <= maxCustomerId).ToList();
                    }
                    break;
                case "CardNumber":
                    if (!string.IsNullOrWhiteSpace(minValue))
                    {
                        paymentMethods = paymentMethods.Where(p => p.CardNumber.Contains(minValue)).ToList();
                    }
                    break;
                case "ExpirationDate":
                    if (DateTime.TryParse(minValue, out DateTime minExpirationDate) && DateTime.TryParse(maxValue, out DateTime maxExpirationDate))
                    {
                        paymentMethods = paymentMethods.Where(p => p.ExpirationDate.Date >= minExpirationDate.Date && p.ExpirationDate.Date <= maxExpirationDate.Date).ToList();
                    }
                    else if (DateTime.TryParse(minValue, out minExpirationDate))
                    {
                        paymentMethods = paymentMethods.Where(p => p.ExpirationDate.Date >= minExpirationDate.Date).ToList();
                    }
                    else if (DateTime.TryParse(maxValue, out maxExpirationDate))
                    {
                        paymentMethods = paymentMethods.Where(p => p.ExpirationDate.Date <= maxExpirationDate.Date).ToList();
                    }
                    break;
            }
            PaymentMethodsCollectionView.ItemsSource = paymentMethods;
        }

        private void OnFilterByCustomerIdClicked(object sender, EventArgs e)
        {
            FilterPaymentMethods("CustomerId", MinCustomerIdEntry.Text, MaxCustomerIdEntry.Text);
        }

        private void OnFilterByCardNumberClicked(object sender, EventArgs e)
        {
            FilterPaymentMethods("CardNumber", MinCardNumberEntry.Text, MaxCardNumberEntry.Text);
        }

        private void OnFilterByExpirationDateClicked(object sender, EventArgs e)
        {
            FilterPaymentMethods("ExpirationDate", MinExpirationDateEntry.Text, MaxExpirationDateEntry.Text);
        }

        private void OnRefreshFiltersClicked(object sender, EventArgs e)
        {
            // Clear all filter inputs
            MinCustomerIdEntry.Text = string.Empty;
            MaxCustomerIdEntry.Text = string.Empty;
            MinCardNumberEntry.Text = string.Empty;
            MaxCardNumberEntry.Text = string.Empty;
            MinExpirationDateEntry.Text = string.Empty;
            MaxExpirationDateEntry.Text = string.Empty;

            // Reset the displayed payment methods to the full list
            PaymentMethodsCollectionView.ItemsSource = _masterPaymentMethodList;
        }

        protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}