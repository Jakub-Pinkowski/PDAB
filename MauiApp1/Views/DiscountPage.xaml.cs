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
    public partial class DiscountPage : ContentPage, INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private Discount? _editingDiscount;
        private string _buttonText = "Add Discount";
        private bool _isEditing = false;
        private bool _isSortedAscending = true;
        private List<Discount> _masterDiscountList = new List<Discount>();

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
            _masterDiscountList = await _databaseService.GetItemsAsync<Discount>();
            DiscountsCollectionView.ItemsSource = _masterDiscountList;
        }

        private async void OnAddDiscountClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CodeEntry.Text) ||
                string.IsNullOrWhiteSpace(PercentageEntry.Text) || !decimal.TryParse(PercentageEntry.Text, out var percentage) || percentage <= 0 ||
                string.IsNullOrWhiteSpace(ExpirationDateEntry.Text) || !DateTime.TryParse(ExpirationDateEntry.Text, out var expirationDate))
            {
                await DisplayAlert("Validation Error", "Please ensure all fields are filled correctly. Percentage must be a positive number, and Expiration Date must be a valid date.", "OK");
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
                bool confirm = await DisplayAlert("Confirm Delete", $"Are you sure you want to delete the discount with Code {discount.Code}?", "Yes", "No");
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

        private void SortDiscounts(string criterion)
        {
            var discounts = DiscountsCollectionView.ItemsSource.Cast<Discount>().ToList();
            switch (criterion)
            {
                case "Code":
                    discounts = _isSortedAscending ? discounts.OrderBy(d => d.Code).ToList() : discounts.OrderByDescending(d => d.Code).ToList();
                    _isSortedAscending = !_isSortedAscending;
                    break;
                case "Percentage":
                    discounts = _isSortedAscending ? discounts.OrderBy(d => d.Percentage).ToList() : discounts.OrderByDescending(d => d.Percentage).ToList();
                    _isSortedAscending = !_isSortedAscending;
                    break;
                case "ExpirationDate":
                    discounts = _isSortedAscending ? discounts.OrderBy(d => d.ExpirationDate).ToList() : discounts.OrderByDescending(d => d.ExpirationDate).ToList();
                    _isSortedAscending = !_isSortedAscending;
                    break;
            }
            DiscountsCollectionView.ItemsSource = discounts;
        }

        private void OnSortByCodeClicked(object sender, EventArgs e)
        {
            SortDiscounts("Code");
        }

        private void OnSortByPercentageClicked(object sender, EventArgs e)
        {
            SortDiscounts("Percentage");
        }

        private void OnSortByExpirationDateClicked(object sender, EventArgs e)
        {
            SortDiscounts("ExpirationDate");
        }

        private void FilterDiscounts(string criterion, string minValue, string maxValue)
        {
            var discounts = _masterDiscountList;
            switch (criterion)
            {
                case "Code":
                    if (!string.IsNullOrWhiteSpace(minValue))
                    {
                        discounts = discounts.Where(d => d.Code.Contains(minValue)).ToList();
                    }
                    break;
                case "Percentage":
                    if (decimal.TryParse(minValue, out decimal minPercentage) && decimal.TryParse(maxValue, out decimal maxPercentage))
                    {
                        discounts = discounts.Where(d => d.Percentage >= minPercentage && d.Percentage <= maxPercentage).ToList();
                    }
                    else if (decimal.TryParse(minValue, out minPercentage))
                    {
                        discounts = discounts.Where(d => d.Percentage >= minPercentage).ToList();
                    }
                    else if (decimal.TryParse(maxValue, out maxPercentage))
                    {
                        discounts = discounts.Where(d => d.Percentage <= maxPercentage).ToList();
                    }
                    break;
                case "ExpirationDate":
                    if (DateTime.TryParse(minValue, out DateTime minExpirationDate) && DateTime.TryParse(maxValue, out DateTime maxExpirationDate))
                    {
                        discounts = discounts.Where(d => d.ExpirationDate.Date >= minExpirationDate.Date && d.ExpirationDate.Date <= maxExpirationDate.Date).ToList();
                    }
                    else if (DateTime.TryParse(minValue, out minExpirationDate))
                    {
                        discounts = discounts.Where(d => d.ExpirationDate.Date >= minExpirationDate.Date).ToList();
                    }
                    else if (DateTime.TryParse(maxValue, out maxExpirationDate))
                    {
                        discounts = discounts.Where(d => d.ExpirationDate.Date <= maxExpirationDate.Date).ToList();
                    }
                    break;
            }
            DiscountsCollectionView.ItemsSource = discounts;
        }

        private void OnFilterByCodeClicked(object sender, EventArgs e)
        {
            FilterDiscounts("Code", MinCodeEntry.Text, MaxCodeEntry.Text);
        }

        private void OnFilterByPercentageClicked(object sender, EventArgs e)
        {
            FilterDiscounts("Percentage", MinPercentageEntry.Text, MaxPercentageEntry.Text);
        }

        private void OnFilterByExpirationDateClicked(object sender, EventArgs e)
        {
            FilterDiscounts("ExpirationDate", MinExpirationDateEntry.Text, MaxExpirationDateEntry.Text);
        }

        private void OnRefreshFiltersClicked(object sender, EventArgs e)
        {
            // Clear all filter inputs
            MinCodeEntry.Text = string.Empty;
            MaxCodeEntry.Text = string.Empty;
            MinPercentageEntry.Text = string.Empty;
            MaxPercentageEntry.Text = string.Empty;
            MinExpirationDateEntry.Text = string.Empty;
            MaxExpirationDateEntry.Text = string.Empty;

            // Reset the displayed discounts to the full list
            DiscountsCollectionView.ItemsSource = _masterDiscountList;
        }
    }
}