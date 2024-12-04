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
    public partial class AddressPage : ContentPage, INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private Address? _editingAddress;
        private string _buttonText = "Add Address";
        private bool _isEditing = false;

        public new event PropertyChangedEventHandler? PropertyChanged;

        public AddressPage(DatabaseService databaseService)
        {
            InitializeComponent();
            BindingContext = this;
            _databaseService = databaseService;
            LoadAddressesAsync();
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

        private async void LoadAddressesAsync()
        {
            var addresses = await _databaseService.GetItemsAsync<Address>();
            AddressesCollectionView.ItemsSource = addresses;
        }

        private async void OnAddAddressClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CustomerIdEntry.Text) || !int.TryParse(CustomerIdEntry.Text, out var customerId) || customerId <= 0 ||
                string.IsNullOrWhiteSpace(StreetEntry.Text) || StreetEntry.Text.Length < 2 ||
                string.IsNullOrWhiteSpace(CityEntry.Text) || CityEntry.Text.Length < 2 ||
                string.IsNullOrWhiteSpace(StateEntry.Text) || StateEntry.Text.Length < 2 ||
                string.IsNullOrWhiteSpace(ZipCodeEntry.Text) || ZipCodeEntry.Text.Length < 2)
            {
                await DisplayAlert("Validation Error", "Please ensure all fields are filled correctly.", "OK");
                return;
            }

            if (_editingAddress == null)
            {
                var newAddress = new Address
                {
                    CustomerId = customerId,
                    Street = StreetEntry.Text,
                    City = CityEntry.Text,
                    State = StateEntry.Text,
                    ZipCode = ZipCodeEntry.Text
                };

                await _databaseService.SaveItemAsync(newAddress);
            }
            else
            {
                _editingAddress.CustomerId = customerId;
                _editingAddress.Street = StreetEntry.Text;
                _editingAddress.City = CityEntry.Text;
                _editingAddress.State = StateEntry.Text;
                _editingAddress.ZipCode = ZipCodeEntry.Text;
                await _databaseService.SaveItemAsync(_editingAddress);
                _editingAddress = null;
                ButtonText = "Add Address";
                IsEditing = false;
            }

            LoadAddressesAsync();

            // Reset the input fields
            CustomerIdEntry.Text = string.Empty;
            StreetEntry.Text = string.Empty;
            CityEntry.Text = string.Empty;
            StateEntry.Text = string.Empty;
            ZipCodeEntry.Text = string.Empty;
        }

        private async void OnDeleteAddressClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var address = button?.CommandParameter as Address;

            if (address != null)
            {
                bool confirm = await DisplayAlert("Confirm Delete", $"Are you sure you want to delete the address at {address.Street}?", "Yes", "No");
                if (confirm)
                {
                    await _databaseService.DeleteItemAsync(address);
                    LoadAddressesAsync();
                }
            }
        }

        private void OnEditAddressClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var address = button?.CommandParameter as Address;

            if (address != null)
            {
                _editingAddress = address;
                CustomerIdEntry.Text = address.CustomerId.ToString();
                StreetEntry.Text = address.Street;
                CityEntry.Text = address.City;
                StateEntry.Text = address.State;
                ZipCodeEntry.Text = address.ZipCode;
                ButtonText = "Edit Address";
                IsEditing = true;
            }
        }

        private void OnCancelEditClicked(object sender, EventArgs e)
        {
            _editingAddress = null;
            ButtonText = "Add Address";
            IsEditing = false;

            // Reset the input fields
            CustomerIdEntry.Text = string.Empty;
            StreetEntry.Text = string.Empty;
            CityEntry.Text = string.Empty;
            StateEntry.Text = string.Empty;
            ZipCodeEntry.Text = string.Empty;
        }

        private void OnRefreshClicked(object sender, EventArgs e)
        {
            LoadDiscountsAsync();
        }

        protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}