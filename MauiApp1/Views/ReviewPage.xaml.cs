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
    public partial class ReviewPage : ContentPage, INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private Review? _editingReview;
        private string _buttonText = "Add Review";
        private bool _isEditing = false;
        private bool _isSortedAscending = true;
        private List<Review> _masterReviewList = new List<Review>();

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
            _masterReviewList = await _databaseService.GetItemsAsync<Review>();
            ReviewsCollectionView.ItemsSource = _masterReviewList;
        }

        private async void OnAddReviewClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ProductIdEntry.Text) || !int.TryParse(ProductIdEntry.Text, out var productId) || productId <= 0 ||
                string.IsNullOrWhiteSpace(CustomerIdEntry.Text) || !int.TryParse(CustomerIdEntry.Text, out var customerId) || customerId <= 0 ||
                string.IsNullOrWhiteSpace(RatingEntry.Text) || !int.TryParse(RatingEntry.Text, out var rating) || rating < 1 || rating > 5 ||
                string.IsNullOrWhiteSpace(CommentEntry.Text) || CommentEntry.Text.Length < 2)
            {
                await DisplayAlert("Validation Error", "Please ensure all fields are filled correctly. Rating must be between 1 and 5.", "OK");
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
                bool confirm = await DisplayAlert("Confirm Delete", $"Are you sure you want to delete the review for Product ID {review.ProductId}?", "Yes", "No");
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

        private void SortReviews(string criterion)
        {
            var reviews = ReviewsCollectionView.ItemsSource.Cast<Review>().ToList();
            switch (criterion)
            {
                case "ProductId":
                    reviews = _isSortedAscending ? reviews.OrderBy(r => r.ProductId).ToList() : reviews.OrderByDescending(r => r.ProductId).ToList();
                    _isSortedAscending = !_isSortedAscending;
                    break;
                case "CustomerId":
                    reviews = _isSortedAscending ? reviews.OrderBy(r => r.CustomerId).ToList() : reviews.OrderByDescending(r => r.CustomerId).ToList();
                    _isSortedAscending = !_isSortedAscending;
                    break;
                case "Rating":
                    reviews = _isSortedAscending ? reviews.OrderBy(r => r.Rating).ToList() : reviews.OrderByDescending(r => r.Rating).ToList();
                    _isSortedAscending = !_isSortedAscending;
                    break;
            }
            ReviewsCollectionView.ItemsSource = reviews;
        }

        private void OnSortByProductIdClicked(object sender, EventArgs e)
        {
            SortReviews("ProductId");
        }

        private void OnSortByCustomerIdClicked(object sender, EventArgs e)
        {
            SortReviews("CustomerId");
        }

        private void OnSortByRatingClicked(object sender, EventArgs e)
        {
            SortReviews("Rating");
        }

        private void FilterReviews(string criterion, string minValue, string maxValue)
        {
            var reviews = _masterReviewList;
            switch (criterion)
            {
                case "ProductId":
                    if (int.TryParse(minValue, out int minProductId) && int.TryParse(maxValue, out int maxProductId))
                    {
                        reviews = reviews.Where(r => r.ProductId >= minProductId && r.ProductId <= maxProductId).ToList();
                    }
                    else if (int.TryParse(minValue, out minProductId))
                    {
                        reviews = reviews.Where(r => r.ProductId >= minProductId).ToList();
                    }
                    else if (int.TryParse(maxValue, out maxProductId))
                    {
                        reviews = reviews.Where(r => r.ProductId <= maxProductId).ToList();
                    }
                    break;
                case "CustomerId":
                    if (int.TryParse(minValue, out int minCustomerId) && int.TryParse(maxValue, out int maxCustomerId))
                    {
                        reviews = reviews.Where(r => r.CustomerId >= minCustomerId && r.CustomerId <= maxCustomerId).ToList();
                    }
                    else if (int.TryParse(minValue, out minCustomerId))
                    {
                        reviews = reviews.Where(r => r.CustomerId >= minCustomerId).ToList();
                    }
                    else if (int.TryParse(maxValue, out maxCustomerId))
                    {
                        reviews = reviews.Where(r => r.CustomerId <= maxCustomerId).ToList();
                    }
                    break;
                case "Rating":
                    if (int.TryParse(minValue, out int minRating) && int.TryParse(maxValue, out int maxRating))
                    {
                        reviews = reviews.Where(r => r.Rating >= minRating && r.Rating <= maxRating).ToList();
                    }
                    else if (int.TryParse(minValue, out minRating))
                    {
                        reviews = reviews.Where(r => r.Rating >= minRating).ToList();
                    }
                    else if (int.TryParse(maxValue, out maxRating))
                    {
                        reviews = reviews.Where(r => r.Rating <= maxRating).ToList();
                    }
                    break;
            }
            ReviewsCollectionView.ItemsSource = reviews;
        }

        private void OnFilterByProductIdClicked(object sender, EventArgs e)
        {
            FilterReviews("ProductId", MinProductIdEntry.Text, MaxProductIdEntry.Text);
        }

        private void OnFilterByCustomerIdClicked(object sender, EventArgs e)
        {
            FilterReviews("CustomerId", MinCustomerIdEntry.Text, MaxCustomerIdEntry.Text);
        }

        private void OnFilterByRatingClicked(object sender, EventArgs e)
        {
            FilterReviews("Rating", MinRatingEntry.Text, MaxRatingEntry.Text);
        }

        private void OnRefreshFiltersClicked(object sender, EventArgs e)
        {
            // Clear all filter inputs
            MinProductIdEntry.Text = string.Empty;
            MaxProductIdEntry.Text = string.Empty;
            MinCustomerIdEntry.Text = string.Empty;
            MaxCustomerIdEntry.Text = string.Empty;
            MinRatingEntry.Text = string.Empty;
            MaxRatingEntry.Text = string.Empty;

            // Reset the displayed reviews to the full list
            ReviewsCollectionView.ItemsSource = _masterReviewList;
        }

        protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}