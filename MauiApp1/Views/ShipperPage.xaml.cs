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
    public partial class ShipperPage : ContentPage, INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private Shipper? _editingShipper;
        private string _buttonText = "Add Shipper";
        private bool _isEditing = false;
        private bool _isSortedAscending = true;
        private List<Shipper> _masterShipperList = new List<Shipper>();

        public new event PropertyChangedEventHandler? PropertyChanged;

        public ShipperPage(DatabaseService databaseService)
        {
            InitializeComponent();
            BindingContext = this;
            _databaseService = databaseService;
            LoadShippersAsync();
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

        private async void LoadShippersAsync()
        {
            _masterShipperList = await _databaseService.GetItemsAsync<Shipper>();
            ShippersCollectionView.ItemsSource = _masterShipperList;
        }

        private async void OnAddShipperClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameEntry.Text) || NameEntry.Text.Length < 2 ||
                string.IsNullOrWhiteSpace(PhoneEntry.Text) || PhoneEntry.Text.Length < 2)
            {
                await DisplayAlert("Validation Error", "Name and Phone must be at least 2 characters long.", "OK");
                return;
            }

            if (_editingShipper == null)
            {
                var newShipper = new Shipper
                {
                    Name = NameEntry.Text,
                    Phone = PhoneEntry.Text
                };

                await _databaseService.SaveItemAsync(newShipper);
            }
            else
            {
                _editingShipper.Name = NameEntry.Text;
                _editingShipper.Phone = PhoneEntry.Text;
                await _databaseService.SaveItemAsync(_editingShipper);
                _editingShipper = null;
                ButtonText = "Add Shipper";
                IsEditing = false;
            }

            LoadShippersAsync();

            // Reset the input fields
            NameEntry.Text = string.Empty;
            PhoneEntry.Text = string.Empty;
        }

        private async void OnDeleteShipperClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var shipper = button?.CommandParameter as Shipper;

            if (shipper != null)
            {
                bool confirm = await DisplayAlert("Confirm Delete", $"Are you sure you want to delete {shipper.Name}?", "Yes", "No");
                if (confirm)
                {
                    await _databaseService.DeleteItemAsync(shipper);
                    LoadShippersAsync();
                }
            }
        }

        private void OnEditShipperClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var shipper = button?.CommandParameter as Shipper;

            if (shipper != null)
            {
                _editingShipper = shipper;
                NameEntry.Text = shipper.Name;
                PhoneEntry.Text = shipper.Phone;
                ButtonText = "Edit Shipper";
                IsEditing = true;
            }
        }

        private void OnCancelEditClicked(object sender, EventArgs e)
        {
            _editingShipper = null;
            ButtonText = "Add Shipper";
            IsEditing = false;

            // Reset the input fields
            NameEntry.Text = string.Empty;
            PhoneEntry.Text = string.Empty;
        }

        private void OnRefreshClicked(object sender, EventArgs e)
        {
            LoadShippersAsync();
        }

        private void SortShippers(string criterion)
        {
            var shippers = ShippersCollectionView.ItemsSource.Cast<Shipper>().ToList();
            switch (criterion)
            {
                case "Name":
                    shippers = _isSortedAscending ? shippers.OrderBy(s => s.Name).ToList() : shippers.OrderByDescending(s => s.Name).ToList();
                    _isSortedAscending = !_isSortedAscending;
                    break;
                case "Phone":
                    shippers = _isSortedAscending ? shippers.OrderBy(s => s.Phone).ToList() : shippers.OrderByDescending(s => s.Phone).ToList();
                    _isSortedAscending = !_isSortedAscending;
                    break;
            }
            ShippersCollectionView.ItemsSource = shippers;
        }

        private void OnSortByNameClicked(object sender, EventArgs e)
        {
            SortShippers("Name");
        }

        private void OnSortByPhoneClicked(object sender, EventArgs e)
        {
            SortShippers("Phone");
        }

        private void FilterShippers(string criterion, string minValue, string maxValue)
        {
            var shippers = _masterShipperList;
            switch (criterion)
            {
                case "Name":
                    if (!string.IsNullOrWhiteSpace(minValue))
                    {
                        shippers = shippers.Where(s => s.Name.Contains(minValue)).ToList();
                    }
                    break;
                case "Phone":
                    if (!string.IsNullOrWhiteSpace(minValue))
                    {
                        shippers = shippers.Where(s => s.Phone.Contains(minValue)).ToList();
                    }
                    break;
            }
            ShippersCollectionView.ItemsSource = shippers;
        }

        private void OnFilterByNameClicked(object sender, EventArgs e)
        {
            FilterShippers("Name", MinNameEntry.Text, MaxNameEntry.Text);
        }

        private void OnFilterByPhoneClicked(object sender, EventArgs e)
        {
            FilterShippers("Phone", MinPhoneEntry.Text, MaxPhoneEntry.Text);
        }

        private void OnRefreshFiltersClicked(object sender, EventArgs e)
        {
            // Clear all filter inputs
            MinNameEntry.Text = string.Empty;
            MaxNameEntry.Text = string.Empty;
            MinPhoneEntry.Text = string.Empty;
            MaxPhoneEntry.Text = string.Empty;

            // Reset the displayed shippers to the full list
            ShippersCollectionView.ItemsSource = _masterShipperList;
        }

        protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}