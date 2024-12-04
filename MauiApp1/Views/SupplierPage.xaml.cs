using MauiApp1.Models;
using MauiApp1.Services;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MauiApp1
{
    public partial class SupplierPage : ContentPage, INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private Supplier? _editingSupplier;
        private string _buttonText = "Add Supplier";
        private bool _isEditing = false;

        public new event PropertyChangedEventHandler? PropertyChanged;

        public SupplierPage(DatabaseService databaseService)
        {
            InitializeComponent();
            BindingContext = this;
            _databaseService = databaseService;
            LoadSuppliersAsync();
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

        private async void LoadSuppliersAsync()
        {
            var suppliers = await _databaseService.GetItemsAsync<Supplier>();
            SuppliersCollectionView.ItemsSource = suppliers;
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Use Regex to check if the email is in a valid format
                var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return emailRegex.IsMatch(email);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async void OnAddSupplierClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameEntry.Text) || NameEntry.Text.Length < 2 ||
                string.IsNullOrWhiteSpace(ContactEmailEntry.Text) || !IsValidEmail(ContactEmailEntry.Text))
            {
                await DisplayAlert("Validation Error", "Name must be at least 2 characters long and Email must be a valid email address.", "OK");
                return;
            }

            if (_editingSupplier == null)
            {
                var newSupplier = new Supplier
                {
                    Name = NameEntry.Text,
                    ContactEmail = ContactEmailEntry.Text
                };

                await _databaseService.SaveItemAsync(newSupplier);
            }
            else
            {
                _editingSupplier.Name = NameEntry.Text;
                _editingSupplier.ContactEmail = ContactEmailEntry.Text;
                await _databaseService.SaveItemAsync(_editingSupplier);
                _editingSupplier = null;
                ButtonText = "Add Supplier";
                IsEditing = false;
            }

            LoadSuppliersAsync();

            // Reset the input fields
            NameEntry.Text = string.Empty;
            ContactEmailEntry.Text = string.Empty;
        }

        private async void OnDeleteSupplierClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var supplier = button?.CommandParameter as Supplier;

            if (supplier != null)
            {
                bool confirm = await DisplayAlert("Confirm Delete", $"Are you sure you want to delete the supplier {supplier.Name}?", "Yes", "No");
                if (confirm)
                {
                    await _databaseService.DeleteItemAsync(supplier);
                    LoadSuppliersAsync();
                }
            }
        }

        private void OnEditSupplierClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var supplier = button?.CommandParameter as Supplier;

            if (supplier != null)
            {
                _editingSupplier = supplier;
                NameEntry.Text = supplier.Name;
                ContactEmailEntry.Text = supplier.ContactEmail;
                ButtonText = "Edit Supplier";
                IsEditing = true;
            }
        }

        private void OnCancelEditClicked(object sender, EventArgs e)
        {
            _editingSupplier = null;
            ButtonText = "Add Supplier";
            IsEditing = false;

            // Reset the input fields
            NameEntry.Text = string.Empty;
            ContactEmailEntry.Text = string.Empty;
        }

        private void OnRefreshClicked(object sender, EventArgs e)
        {
            LoadSuppliersAsync();
        }

        protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}