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
    public partial class ShipperPage : ContentPage, INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private Shipper? _editingShipper;
        private string _buttonText = "Add Shipper";
        private bool _isEditing = false;

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
            var shippers = await _databaseService.GetItemsAsync<Shipper>();
            ShippersCollectionView.ItemsSource = shippers;
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
                bool confirm = await DisplayAlert("Confirm Delete", $"Are you sure you want to delete the shipper {shipper.Name}?", "Yes", "No");
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

        protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}