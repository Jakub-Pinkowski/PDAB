using MauiApp1.Models;
using MauiApp1.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace MauiApp1
{
    public partial class MainPage : ContentPage
    {
        private readonly DatabaseService _databaseService;

        public MainPage(DatabaseService databaseService)
        {
            InitializeComponent();
            _databaseService = databaseService;
            LoadData();
        }

        private async void OnBackupDatabaseClicked(object sender, EventArgs e)
        {
            try
            {
                var backupPath = Path.Combine(FileSystem.AppDataDirectory, "database_backup.db");

                await _databaseService.BackupDatabaseAsync(backupPath);
                await DisplayAlert("Success", $"Database backup created successfully at {backupPath}.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to create database backup: {ex.Message}", "OK");
            }
        }

        private async void OnResetDatabaseClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Confirm Reset", "Are you sure you want to reset the database? This action cannot be undone.", "Yes", "No");
            if (confirm)
            {
                await _databaseService.ResetDatabaseAsync();
                await DisplayAlert("Success", "Database has been reset.", "OK");
                // Reload the data after resetting the database
                LoadData();
            }
        }

        private void OnRefreshDataClicked(object sender, EventArgs e)
        {
            LoadData();
        }

        private async void LoadData()
        {
            await LoadCustomerOrders();
            await LoadProductReviews();
            await LoadOrderDetails();
        }

        private async Task LoadCustomerOrders()
        {
            var customerOrders = await _databaseService.GetCustomerOrdersAsync();

            var customerSpending = customerOrders
                .GroupBy(co => new { co.CustomerName, co.CustomerEmail })
                .Select(g => new
                {
                    CustomerName = g.Key.CustomerName,
                    TotalAmount = g.Sum(co => co.TotalAmount)
                })
                .OrderByDescending(cs => cs.TotalAmount)
                .ToList();

            var maxAmount = customerSpending.Max(cs => cs.TotalAmount);

            CustomerSpendingChart.Children.Clear();

            foreach (var cs in customerSpending)
            {
                var bar = new BoxView
                {
                    HeightRequest = 20,
                    WidthRequest = (double)(cs.TotalAmount / maxAmount) * 300,
                    Color = Colors.Blue,
                    HorizontalOptions = LayoutOptions.Start
                };

                var label = new Label
                {
                    Text = $"{cs.CustomerName}: {cs.TotalAmount:C}",
                    VerticalOptions = LayoutOptions.Center
                };

                var stack = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    Children = { bar, label }
                };

                CustomerSpendingChart.Children.Add(stack);
            }
        }

        private async Task LoadProductReviews()
        {
            var productReviews = await _databaseService.GetProductReviewsAsync();
            ProductReviewsListView.ItemsSource = productReviews;
        }

        private async Task LoadOrderDetails()
        {
            var orderDetails = await _databaseService.GetOrderDetailsAsync();
            OrderDetailsListView.ItemsSource = orderDetails;
        }
    }
}