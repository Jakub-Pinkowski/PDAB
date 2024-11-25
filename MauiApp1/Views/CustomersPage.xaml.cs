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
    public partial class CustomersPage : ContentPage, INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private Customer? _editingCustomer;
        private string _buttonText = "Add Customer";
        private bool _isEditing = false;

        public new event PropertyChangedEventHandler? PropertyChanged;

        public CustomersPage(DatabaseService databaseService)
        {
            InitializeComponent();
            BindingContext = this;
            _databaseService = databaseService;
            LoadCustomersAsync();
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

        private async void LoadCustomersAsync()
        {
            var customers = await _databaseService.GetItemsAsync<Customer>();
            CustomersCollectionView.ItemsSource = customers;
        }

        private async void OnAddCustomerClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameEntry.Text) || NameEntry.Text.Length < 2 ||
                string.IsNullOrWhiteSpace(EmailEntry.Text) || EmailEntry.Text.Length < 2)
            {
                await DisplayAlert("Validation Error", "Name and Email must be at least 2 characters long.", "OK");
                return;
            }

            if (_editingCustomer == null)
            {
                var newCustomer = new Customer
                {
                    Name = NameEntry.Text,
                    Email = EmailEntry.Text
                };

                await _databaseService.SaveItemAsync(newCustomer);
            }
            else
            {
                _editingCustomer.Name = NameEntry.Text;
                _editingCustomer.Email = EmailEntry.Text;
                await _databaseService.SaveItemAsync(_editingCustomer);
                _editingCustomer = null;
                ButtonText = "Add Customer";
                IsEditing = false;
            }

            LoadCustomersAsync();

            // Reset the input fields
            NameEntry.Text = string.Empty;
            EmailEntry.Text = string.Empty;
        }

        private async void OnDeleteCustomerClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var customer = button?.CommandParameter as Customer;

            if (customer != null)
            {
                bool confirm = await DisplayAlert("Confirm Delete", $"Are you sure you want to delete {customer.Name}?", "Yes", "No");
                if (confirm)
                {
                    await _databaseService.DeleteItemAsync(customer);
                    LoadCustomersAsync();
                }
            }
        }

        private void OnEditCustomerClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var customer = button?.CommandParameter as Customer;

            if (customer != null)
            {
                _editingCustomer = customer;
                NameEntry.Text = customer.Name;
                EmailEntry.Text = customer.Email;
                ButtonText = "Edit Customer";
                IsEditing = true;
            }
        }

        private void OnCancelEditClicked(object sender, EventArgs e)
        {
            _editingCustomer = null;
            ButtonText = "Add Customer";
            IsEditing = false;

            // Reset the input fields
            NameEntry.Text = string.Empty;
            EmailEntry.Text = string.Empty;
        }

        protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}