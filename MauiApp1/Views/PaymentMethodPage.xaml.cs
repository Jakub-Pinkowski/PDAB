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
    public partial class PaymentMethodPage : ContentPage, INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private PaymentMethod? _editingPaymentMethod;
        private string _buttonText = "Add Payment Method";
        private bool _isEditing = false;

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
            var paymentMethods = await _databaseService.GetItemsAsync<PaymentMethod>();
            PaymentMethodsCollectionView.ItemsSource = paymentMethods;
        }

        private async void OnAddPaymentMethodClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CustomerIdEntry.Text) || !int.TryParse(CustomerIdEntry.Text, out var customerId) || customerId <= 0 ||
                string.IsNullOrWhiteSpace(CardNumberEntry.Text) || CardNumberEntry.Text.Length < 2 ||
                string.IsNullOrWhiteSpace(ExpirationDateEntry.Text) || !DateTime.TryParse(ExpirationDateEntry.Text, out var expirationDate))
            {
                await DisplayAlert("Validation Error", "Please ensure all fields are filled correctly. Customer ID must be a positive integer, Card Number must be at least 2 characters long, and Expiration Date must be a valid date.", "OK");
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
                bool confirm = await DisplayAlert("Confirm Delete", $"Are you sure you want to delete the payment method with card number {paymentMethod.CardNumber}?", "Yes", "No");
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

        protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}