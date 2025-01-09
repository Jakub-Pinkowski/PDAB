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
    public partial class CustomersPage : ContentPage, INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private Customer? _editingCustomer;
        private string _buttonText = "Add Customer";
        private bool _isEditing = false;
        private bool _isSortedAscending = true;
        private List<Customer> _masterCustomerList = new List<Customer>();

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
            _masterCustomerList = await _databaseService.GetItemsAsync<Customer>();
            CustomersCollectionView.ItemsSource = _masterCustomerList;
        }

        private async void OnAddCustomerClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameEntry.Text) ||
                string.IsNullOrWhiteSpace(EmailEntry.Text))
            {
                await DisplayAlert("Validation Error", "Please ensure all fields are filled correctly.", "OK");
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
                bool confirm = await DisplayAlert("Confirm Delete", $"Are you sure you want to delete the customer with Name {customer.Name}?", "Yes", "No");
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

        private void OnRefreshClicked(object sender, EventArgs e)
        {
            LoadCustomersAsync();
        }

        private void SortCustomers(string criterion)
        {
            var customers = CustomersCollectionView.ItemsSource.Cast<Customer>().ToList();
            switch (criterion)
            {
                case "Name":
                    customers = _isSortedAscending ? customers.OrderBy(c => c.Name).ToList() : customers.OrderByDescending(c => c.Name).ToList();
                    _isSortedAscending = !_isSortedAscending;
                    break;
                case "Email":
                    customers = _isSortedAscending ? customers.OrderBy(c => c.Email).ToList() : customers.OrderByDescending(c => c.Email).ToList();
                    _isSortedAscending = !_isSortedAscending;
                    break;
            }
            CustomersCollectionView.ItemsSource = customers;
        }

        private void OnSortByNameClicked(object sender, EventArgs e)
        {
            SortCustomers("Name");
        }

        private void OnSortByEmailClicked(object sender, EventArgs e)
        {
            SortCustomers("Email");
        }

        private void FilterCustomers(string criterion, string minValue, string maxValue)
        {
            var customers = _masterCustomerList;
            switch (criterion)
            {
                case "Name":
                    if (!string.IsNullOrWhiteSpace(minValue))
                    {
                        customers = customers.Where(c => c.Name.Contains(minValue)).ToList();
                    }
                    break;
                case "Email":
                    if (!string.IsNullOrWhiteSpace(minValue))
                    {
                        customers = customers.Where(c => c.Email.Contains(minValue)).ToList();
                    }
                    break;
            }
            CustomersCollectionView.ItemsSource = customers;
        }

        private void OnFilterByNameClicked(object sender, EventArgs e)
        {
            FilterCustomers("Name", MinNameEntry.Text, MaxNameEntry.Text);
        }

        private void OnFilterByEmailClicked(object sender, EventArgs e)
        {
            FilterCustomers("Email", MinEmailEntry.Text, MaxEmailEntry.Text);
        }

        private void OnRefreshFiltersClicked(object sender, EventArgs e)
        {
            // Clear all filter inputs
            MinNameEntry.Text = string.Empty;
            MaxNameEntry.Text = string.Empty;
            MinEmailEntry.Text = string.Empty;
            MaxEmailEntry.Text = string.Empty;

            // Reset the displayed customers to the full list
            CustomersCollectionView.ItemsSource = _masterCustomerList;
        }
    }
}