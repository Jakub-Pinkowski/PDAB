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
    public partial class DiscountPage : ContentPage, INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private Discount? _editingDiscount;
        private string _buttonText = "Add Discount";
        private bool _isEditing = false;

        public new event PropertyChangedEventHandler? PropertyChanged;

        public DiscountPage(DatabaseService databaseService)
        {
            InitializeComponent();
            BindingContext = this;
            _databaseService = databaseService;
            LoadDiscountsAsync();
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

        private async void LoadDiscountsAsync()
        {
            var discounts = await _databaseService.GetItemsAsync<Discount>();
            DiscountsCollectionView.ItemsSource = discounts;
        }

        private async void OnAddDiscountClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CodeEntry.Text) || CodeEntry.Text.Length < 2 ||
                string.IsNullOrWhiteSpace(PercentageEntry.Text) || !decimal.TryParse(PercentageEntry.Text, out var percentage) || percentage <= 0 ||
                string.IsNullOrWhiteSpace(ExpirationDateEntry.Text) || !DateTime.TryParse(ExpirationDateEntry.Text, out var expirationDate))
            {
                await DisplayAlert("Validation Error", "Please ensure all fields are filled correctly. Code must be at least 2 characters long, Percentage must be a positive number, and Expiration Date must be a valid date.", "OK");
                return;
            }

            if (_editingDiscount == null)
            {
                var newDiscount = new Discount
                {
                    Code = CodeEntry.Text,
                    Percentage = percentage,
                    ExpirationDate = expirationDate
                };

                await _databaseService.SaveItemAsync(newDiscount);
            }
            else
            {
                _editingDiscount.Code = CodeEntry.Text;
                _editingDiscount.Percentage = percentage;
                _editingDiscount.ExpirationDate = expirationDate;
                await _databaseService.SaveItemAsync(_editingDiscount);
                _editingDiscount = null;
                ButtonText = "Add Discount";
                IsEditing = false;
            }

            LoadDiscountsAsync();

            // Reset the input fields
            CodeEntry.Text = string.Empty;
            PercentageEntry.Text = string.Empty;
            ExpirationDateEntry.Text = string.Empty;
        }

        private async void OnDeleteDiscountClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var discount = button?.CommandParameter as Discount;

            if (discount != null)
            {
                bool confirm = await DisplayAlert("Confirm Delete", $"Are you sure you want to delete the discount with code {discount.Code}?", "Yes", "No");
                if (confirm)
                {
                    await _databaseService.DeleteItemAsync(discount);
                    LoadDiscountsAsync();
                }
            }
        }

        private void OnEditDiscountClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var discount = button?.CommandParameter as Discount;

            if (discount != null)
            {
                _editingDiscount = discount;
                CodeEntry.Text = discount.Code;
                PercentageEntry.Text = discount.Percentage.ToString();
                ExpirationDateEntry.Text = discount.ExpirationDate.ToString("yyyy-MM-dd");
                ButtonText = "Edit Discount";
                IsEditing = true;
            }
        }

        private void OnCancelEditClicked(object sender, EventArgs e)
        {
            _editingDiscount = null;
            ButtonText = "Add Discount";
            IsEditing = false;

            // Reset the input fields
            CodeEntry.Text = string.Empty;
            PercentageEntry.Text = string.Empty;
            ExpirationDateEntry.Text = string.Empty;
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