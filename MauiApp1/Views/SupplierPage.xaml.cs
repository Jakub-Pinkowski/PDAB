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
    public partial class SupplierPage : ContentPage, INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private Supplier? _editingSupplier;
        private string _buttonText = "Add Supplier";
        private bool _isEditing = false;
        private bool _isSortedAscending = true;
        private List<Supplier> _masterSupplierList = new List<Supplier>();

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
            _masterSupplierList = await _databaseService.GetItemsAsync<Supplier>();
            SuppliersCollectionView.ItemsSource = _masterSupplierList;
        }

        private async void OnAddSupplierClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameEntry.Text) || NameEntry.Text.Length < 2 ||
                string.IsNullOrWhiteSpace(ContactEmailEntry.Text) || ContactEmailEntry.Text.Length < 2)
            {
                await DisplayAlert("Validation Error", "Name and Contact Email must be at least 2 characters long.", "OK");
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
                bool confirm = await DisplayAlert("Confirm Delete", $"Are you sure you want to delete {supplier.Name}?", "Yes", "No");
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

        private void SortSuppliers(string criterion)
        {
            var suppliers = SuppliersCollectionView.ItemsSource.Cast<Supplier>().ToList();
            switch (criterion)
            {
                case "Name":
                    suppliers = _isSortedAscending ? suppliers.OrderBy(s => s.Name).ToList() : suppliers.OrderByDescending(s => s.Name).ToList();
                    _isSortedAscending = !_isSortedAscending;
                    break;
                case "ContactEmail":
                    suppliers = _isSortedAscending ? suppliers.OrderBy(s => s.ContactEmail).ToList() : suppliers.OrderByDescending(s => s.ContactEmail).ToList();
                    _isSortedAscending = !_isSortedAscending;
                    break;
            }
            SuppliersCollectionView.ItemsSource = suppliers;
        }

        private void OnSortByNameClicked(object sender, EventArgs e)
        {
            SortSuppliers("Name");
        }

        private void OnSortByContactEmailClicked(object sender, EventArgs e)
        {
            SortSuppliers("ContactEmail");
        }

        private void FilterSuppliers(string criterion, string minValue, string maxValue)
        {
            var suppliers = _masterSupplierList;
            switch (criterion)
            {
                case "Name":
                    if (!string.IsNullOrWhiteSpace(minValue))
                    {
                        suppliers = suppliers.Where(s => s.Name.Contains(minValue)).ToList();
                    }
                    break;
                case "ContactEmail":
                    if (!string.IsNullOrWhiteSpace(minValue))
                    {
                        suppliers = suppliers.Where(s => s.ContactEmail.Contains(minValue)).ToList();
                    }
                    break;
            }
            SuppliersCollectionView.ItemsSource = suppliers;
        }

        private void OnFilterByNameClicked(object sender, EventArgs e)
        {
            FilterSuppliers("Name", MinNameEntry.Text, MaxNameEntry.Text);
        }

        private void OnFilterByContactEmailClicked(object sender, EventArgs e)
        {
            FilterSuppliers("ContactEmail", MinContactEmailEntry.Text, MaxContactEmailEntry.Text);
        }

        private void OnRefreshFiltersClicked(object sender, EventArgs e)
        {
            // Clear all filter inputs
            MinNameEntry.Text = string.Empty;
            MaxNameEntry.Text = string.Empty;
            MinContactEmailEntry.Text = string.Empty;
            MaxContactEmailEntry.Text = string.Empty;

            // Reset the displayed suppliers to the full list
            SuppliersCollectionView.ItemsSource = _masterSupplierList;
        }

        protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}