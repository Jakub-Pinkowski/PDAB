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
    public partial class AddressPage : ContentPage, INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private Address? _editingAddress;
        private string _buttonText = "Add Address";
        private bool _isEditing = false;
        private bool _isSortedAscending = true;
        private List<Address> _masterAddressList = new List<Address>();

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
            _masterAddressList = await _databaseService.GetItemsAsync<Address>();
            AddressesCollectionView.ItemsSource = _masterAddressList;
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
            LoadAddressesAsync();
        }

        private void SortAddresses(string criterion)
        {
            var addresses = AddressesCollectionView.ItemsSource.Cast<Address>().ToList();
            switch (criterion)
            {
                case "CustomerId":
                    addresses = _isSortedAscending ? addresses.OrderBy(a => a.CustomerId).ToList() : addresses.OrderByDescending(a => a.CustomerId).ToList();
                    _isSortedAscending = !_isSortedAscending;
                    break;
                case "Street":
                    addresses = _isSortedAscending ? addresses.OrderBy(a => a.Street).ToList() : addresses.OrderByDescending(a => a.Street).ToList();
                    _isSortedAscending = !_isSortedAscending;
                    break;
                case "City":
                    addresses = _isSortedAscending ? addresses.OrderBy(a => a.City).ToList() : addresses.OrderByDescending(a => a.City).ToList();
                    _isSortedAscending = !_isSortedAscending;
                    break;
                case "State":
                    addresses = _isSortedAscending ? addresses.OrderBy(a => a.State).ToList() : addresses.OrderByDescending(a => a.State).ToList();
                    _isSortedAscending = !_isSortedAscending;
                    break;
                case "ZipCode":
                    addresses = _isSortedAscending ? addresses.OrderBy(a => a.ZipCode).ToList() : addresses.OrderByDescending(a => a.ZipCode).ToList();
                    _isSortedAscending = !_isSortedAscending;
                    break;
            }
            AddressesCollectionView.ItemsSource = addresses;
        }

        private void OnSortByCustomerIdClicked(object sender, EventArgs e)
        {
            SortAddresses("CustomerId");
        }

        private void OnSortByStreetClicked(object sender, EventArgs e)
        {
            SortAddresses("Street");
        }

        private void OnSortByCityClicked(object sender, EventArgs e)
        {
            SortAddresses("City");
        }

        private void OnSortByStateClicked(object sender, EventArgs e)
        {
            SortAddresses("State");
        }

        private void OnSortByZipCodeClicked(object sender, EventArgs e)
        {
            SortAddresses("ZipCode");
        }

        private void FilterAddresses(string criterion, string minValue, string maxValue)
        {
            var addresses = _masterAddressList;
            switch (criterion)
            {
                case "CustomerId":
                    if (int.TryParse(minValue, out int minCustomerId) && int.TryParse(maxValue, out int maxCustomerId))
                    {
                        addresses = addresses.Where(a => a.CustomerId >= minCustomerId && a.CustomerId <= maxCustomerId).ToList();
                    }
                    else if (int.TryParse(minValue, out minCustomerId))
                    {
                        addresses = addresses.Where(a => a.CustomerId >= minCustomerId).ToList();
                    }
                    else if (int.TryParse(maxValue, out maxCustomerId))
                    {
                        addresses = addresses.Where(a => a.CustomerId <= maxCustomerId).ToList();
                    }
                    break;
                case "Street":
                    if (!string.IsNullOrWhiteSpace(minValue))
                    {
                        addresses = addresses.Where(a => a.Street.Contains(minValue)).ToList();
                    }
                    break;
                case "City":
                    if (!string.IsNullOrWhiteSpace(minValue))
                    {
                        addresses = addresses.Where(a => a.City.Contains(minValue)).ToList();
                    }
                    break;
                case "State":
                    if (!string.IsNullOrWhiteSpace(minValue))
                    {
                        addresses = addresses.Where(a => a.State.Contains(minValue)).ToList();
                    }
                    break;
                case "ZipCode":
                    if (!string.IsNullOrWhiteSpace(minValue))
                    {
                        addresses = addresses.Where(a => a.ZipCode.Contains(minValue)).ToList();
                    }
                    break;
            }
            AddressesCollectionView.ItemsSource = addresses;
        }

        private void OnFilterByCustomerIdClicked(object sender, EventArgs e)
        {
            FilterAddresses("CustomerId", MinCustomerIdEntry.Text, MaxCustomerIdEntry.Text);
        }

        private void OnFilterByStreetClicked(object sender, EventArgs e)
        {
            FilterAddresses("Street", MinStreetEntry.Text, MaxStreetEntry.Text);
        }

        private void OnFilterByCityClicked(object sender, EventArgs e)
        {
            FilterAddresses("City", MinCityEntry.Text, MaxCityEntry.Text);
        }

        private void OnFilterByStateClicked(object sender, EventArgs e)
        {
            FilterAddresses("State", MinStateEntry.Text, MaxStateEntry.Text);
        }

        private void OnFilterByZipCodeClicked(object sender, EventArgs e)
        {
            FilterAddresses("ZipCode", MinZipCodeEntry.Text, MaxZipCodeEntry.Text);
        }

        private void OnRefreshFiltersClicked(object sender, EventArgs e)
        {
            // Clear all filter inputs
            MinCustomerIdEntry.Text = string.Empty;
            MaxCustomerIdEntry.Text = string.Empty;
            MinStreetEntry.Text = string.Empty;
            MaxStreetEntry.Text = string.Empty;
            MinCityEntry.Text = string.Empty;
            MaxCityEntry.Text = string.Empty;
            MinStateEntry.Text = string.Empty;
            MaxStateEntry.Text = string.Empty;
            MinZipCodeEntry.Text = string.Empty;
            MaxZipCodeEntry.Text = string.Empty;

            // Reset the displayed addresses to the full list
            AddressesCollectionView.ItemsSource = _masterAddressList;
        }

        protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}