using MauiApp1.Models;
using MauiApp1.Services;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MauiApp1
{
    public partial class CustomersPage : ContentPage
    {
        private readonly DatabaseService _databaseService;
        private Customer? _editingCustomer;

        public CustomersPage(DatabaseService databaseService)
        {
            InitializeComponent();
            _databaseService = databaseService;
            LoadCustomersAsync();
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
                    Email = NameEntry.Text
                };

                await _databaseService.SaveItemAsync(newCustomer);
            }
            else
            {
                _editingCustomer.Name = NameEntry.Text;
                _editingCustomer.Email = NameEntry.Text;
                await _databaseService.SaveItemAsync(_editingCustomer);
                _editingCustomer = null;
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
            }
        }
    }
}