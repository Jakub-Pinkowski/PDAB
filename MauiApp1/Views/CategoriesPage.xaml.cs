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
    public partial class CategoriesPage : ContentPage, INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private Category? _editingCategory;
        private string _buttonText = "Add Category";
        private bool _isEditing = false;
        private bool _isSortedAscending = true;
        private List<Category> _masterCategoryList = new List<Category>();

        public new event PropertyChangedEventHandler? PropertyChanged;

        public CategoriesPage(DatabaseService databaseService)
        {
            InitializeComponent();
            BindingContext = this;
            _databaseService = databaseService;
            LoadCategoriesAsync();
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

        private async void LoadCategoriesAsync()
        {
            _masterCategoryList = await _databaseService.GetItemsAsync<Category>();
            CategoriesCollectionView.ItemsSource = _masterCategoryList;
        }

        private async void OnAddCategoryClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameEntry.Text) || NameEntry.Text.Length < 2 ||
                string.IsNullOrWhiteSpace(DescriptionEntry.Text) || DescriptionEntry.Text.Length < 2)
            {
                await DisplayAlert("Validation Error", "Name and Description must be at least 2 characters long.", "OK");
                return;
            }

            if (_editingCategory == null)
            {
                var newCategory = new Category
                {
                    Name = NameEntry.Text,
                    Description = DescriptionEntry.Text,
                    IsActive = IsActiveSwitch.IsToggled
                };

                await _databaseService.SaveItemAsync(newCategory);
            }
            else
            {
                _editingCategory.Name = NameEntry.Text;
                _editingCategory.Description = DescriptionEntry.Text;
                _editingCategory.IsActive = IsActiveSwitch.IsToggled;
                await _databaseService.SaveItemAsync(_editingCategory);
                _editingCategory = null;
                ButtonText = "Add Category";
                IsEditing = false;
            }

            LoadCategoriesAsync();

            // Reset the input fields
            NameEntry.Text = string.Empty;
            DescriptionEntry.Text = string.Empty;
            IsActiveSwitch.IsToggled = true;
        }

        private async void OnDeleteCategoryClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var category = button?.CommandParameter as Category;

            if (category != null)
            {
                bool confirm = await DisplayAlert("Confirm Delete", $"Are you sure you want to delete {category.Name}?", "Yes", "No");
                if (confirm)
                {
                    await _databaseService.DeleteItemAsync(category);
                    LoadCategoriesAsync();
                }
            }
        }

        private void OnEditCategoryClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var category = button?.CommandParameter as Category;

            if (category != null)
            {
                _editingCategory = category;
                NameEntry.Text = category.Name;
                DescriptionEntry.Text = category.Description;
                IsActiveSwitch.IsToggled = category.IsActive;
                ButtonText = "Edit Category";
                IsEditing = true;
            }
        }

        private void OnCancelEditClicked(object sender, EventArgs e)
        {
            _editingCategory = null;
            ButtonText = "Add Category";
            IsEditing = false;

            // Reset the input fields
            NameEntry.Text = string.Empty;
            DescriptionEntry.Text = string.Empty;
            IsActiveSwitch.IsToggled = true;
        }

        private void OnRefreshClicked(object sender, EventArgs e)
        {
            LoadCategoriesAsync();
        }

        private void SortCategories(string criterion)
        {
            var categories = CategoriesCollectionView.ItemsSource.Cast<Category>().ToList();
            switch (criterion)
            {
                case "Name":
                    categories = _isSortedAscending ? categories.OrderBy(c => c.Name).ToList() : categories.OrderByDescending(c => c.Name).ToList();
                    _isSortedAscending = !_isSortedAscending;
                    break;
                case "Description":
                    categories = _isSortedAscending ? categories.OrderBy(c => c.Description).ToList() : categories.OrderByDescending(c => c.Description).ToList();
                    _isSortedAscending = !_isSortedAscending;
                    break;
            }
            CategoriesCollectionView.ItemsSource = categories;
        }

        private void OnSortByNameClicked(object sender, EventArgs e)
        {
            SortCategories("Name");
        }

        private void OnSortByDescriptionClicked(object sender, EventArgs e)
        {
            SortCategories("Description");
        }

        private void FilterCategories(string criterion, string minValue, string maxValue)
        {
            var categories = _masterCategoryList;
            switch (criterion)
            {
                case "Name":
                    if (!string.IsNullOrWhiteSpace(minValue))
                    {
                        categories = categories.Where(c => c.Name.Contains(minValue)).ToList();
                    }
                    break;
                case "Description":
                    if (!string.IsNullOrWhiteSpace(minValue))
                    {
                        categories = categories.Where(c => c.Description.Contains(minValue)).ToList();
                    }
                    break;
            }
            CategoriesCollectionView.ItemsSource = categories;
        }

        private void OnFilterByNameClicked(object sender, EventArgs e)
        {
            FilterCategories("Name", MinNameEntry.Text, MaxNameEntry.Text);
        }

        private void OnFilterByDescriptionClicked(object sender, EventArgs e)
        {
            FilterCategories("Description", MinDescriptionEntry.Text, MaxDescriptionEntry.Text);
        }

        private void OnRefreshFiltersClicked(object sender, EventArgs e)
        {
            // Clear all filter inputs
            MinNameEntry.Text = string.Empty;
            MaxNameEntry.Text = string.Empty;
            MinDescriptionEntry.Text = string.Empty;
            MaxDescriptionEntry.Text = string.Empty;

            // Reset the displayed categories to the full list
            CategoriesCollectionView.ItemsSource = _masterCategoryList;
        }

        protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}