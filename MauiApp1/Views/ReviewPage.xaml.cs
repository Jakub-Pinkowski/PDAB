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
    public partial class ReviewPage : ContentPage, INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private Review? _editingReview;
        private string _buttonText = "Add Review";
        private bool _isEditing = false;

        public new event PropertyChangedEventHandler? PropertyChanged;

        public ReviewPage(DatabaseService databaseService)
        {
            InitializeComponent();
            BindingContext = this;
            _databaseService = databaseService;
            LoadReviewsAsync();
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

        private async void LoadReviewsAsync()
        {
            var reviews = await _databaseService.GetItemsAsync<Review>();
            ReviewsCollectionView.ItemsSource = reviews;
        }

        private async void OnAddReviewClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ProductIdEntry.Text) || !int.TryParse(ProductIdEntry.Text, out var productId) || productId <= 0 ||
                string.IsNullOrWhiteSpace(CustomerIdEntry.Text) || !int.TryParse(CustomerIdEntry.Text, out var customerId) || customerId <= 0 ||
                string.IsNullOrWhiteSpace(RatingEntry.Text) || !int.TryParse(RatingEntry.Text, out var rating) || rating < 1 || rating > 5 ||
                string.IsNullOrWhiteSpace(CommentEntry.Text) || CommentEntry.Text.Length < 2)
            {
                await DisplayAlert("Validation Error", "Please ensure all fields are filled correctly. Product ID and Customer ID must be positive integers, Rating must be between 1 and 5, and Comment must be at least 2 characters long.", "OK");
                return;
            }

            if (_editingReview == null)
            {
                var newReview = new Review
                {
                    ProductId = productId,
                    CustomerId = customerId,
                    Rating = rating,
                    Comment = CommentEntry.Text
                };

                await _databaseService.SaveItemAsync(newReview);
            }
            else
            {
                _editingReview.ProductId = productId;
                _editingReview.CustomerId = customerId;
                _editingReview.Rating = rating;
                _editingReview.Comment = CommentEntry.Text;
                await _databaseService.SaveItemAsync(_editingReview);
                _editingReview = null;
                ButtonText = "Add Review";
                IsEditing = false;
            }

            LoadReviewsAsync();

            // Reset the input fields
            ProductIdEntry.Text = string.Empty;
            CustomerIdEntry.Text = string.Empty;
            RatingEntry.Text = string.Empty;
            CommentEntry.Text = string.Empty;
        }

        private async void OnDeleteReviewClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var review = button?.CommandParameter as Review;

            if (review != null)
            {
                bool confirm = await DisplayAlert("Confirm Delete", $"Are you sure you want to delete the review with ID {review.Id}?", "Yes", "No");
                if (confirm)
                {
                    await _databaseService.DeleteItemAsync(review);
                    LoadReviewsAsync();
                }
            }
        }

        private void OnEditReviewClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var review = button?.CommandParameter as Review;

            if (review != null)
            {
                _editingReview = review;
                ProductIdEntry.Text = review.ProductId.ToString();
                CustomerIdEntry.Text = review.CustomerId.ToString();
                RatingEntry.Text = review.Rating.ToString();
                CommentEntry.Text = review.Comment;
                ButtonText = "Edit Review";
                IsEditing = true;
            }
        }

        private void OnCancelEditClicked(object sender, EventArgs e)
        {
            _editingReview = null;
            ButtonText = "Add Review";
            IsEditing = false;

            // Reset the input fields
            ProductIdEntry.Text = string.Empty;
            CustomerIdEntry.Text = string.Empty;
            RatingEntry.Text = string.Empty;
            CommentEntry.Text = string.Empty;
        }

        private void OnRefreshClicked(object sender, EventArgs e)
        {
            LoadReviewsAsync();
        }

        protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}