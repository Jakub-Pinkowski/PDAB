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
    public partial class CategoriesPage : ContentPage, INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private Category? _editingCategory;
        private string _buttonText = "Add Category";
        private bool _isEditing = false;

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
            var categories = await _databaseService.GetItemsAsync<Category>();
            CategoriesCollectionView.ItemsSource = categories;
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

        protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}