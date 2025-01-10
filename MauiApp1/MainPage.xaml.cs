using MauiApp1.Models;
using MauiApp1.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Net.Http;
using dotenv.net;


namespace MauiApp1
{
    public partial class MainPage : ContentPage
    {
        private readonly DatabaseService _databaseService;
        private ObservableCollection<string> chatMessages = new ObservableCollection<string>();

        public MainPage(DatabaseService databaseService)
        {
            InitializeComponent();
            _databaseService = databaseService;
            LoadData();
            ChatListView.ItemsSource = chatMessages;


            // Load environment variables from .env file
            DotEnv.Load();
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
                LoadData();
            }
        }

        private async void OnSendButtonClicked(object sender, EventArgs e)
        {
            string userInput = UserInputEntry.Text;
            if (!string.IsNullOrEmpty(userInput))
            {
                chatMessages.Add("You: " + userInput);
                UserInputEntry.Text = string.Empty;

                string aiResponse = await GetAIResponse(userInput);
                chatMessages.Add("AI: " + aiResponse);
            }
        }

        private async Task<string> GetAIResponse(string userInput)
        {
            string apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            string apiUrl = "https://api.openai.com/v1/engines/davinci-codex/completions";

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var requestBody = new
                {
                    prompt = userInput,
                    max_tokens = 150
                };

                string jsonRequestBody = JsonSerializer.Serialize(requestBody);
                StringContent content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();
                var jsonDocument = JsonDocument.Parse(jsonResponse);
                string aiResponse = jsonDocument.RootElement.GetProperty("choices")[0].GetProperty("text").GetString();

                return aiResponse.Trim();
            }
        }

        private void OnDarkModeToggled(object sender, ToggledEventArgs e)
        {
            if (e.Value)
            {
                Application.Current.UserAppTheme = AppTheme.Dark;
            }
            else
            {
                Application.Current.UserAppTheme = AppTheme.Light;
            }
        }

        private async void OnRefreshDataClicked(object sender, EventArgs e)
        {
            LoadData();
            await DisplayAlert("Data Refresh", "The database has been refreshed successfully.", "OK");
        }

        private async void LoadData()
        {
            await LoadCustomerTotalAmount();
            await LoadProductReviewChart();
            await LoadOrderItemsChart();
        }

        private async Task LoadCustomerTotalAmount()
        {
            var customerOrders = await _databaseService.GetCustomerTotalAmountsAsync();
            var customerSpending = customerOrders
                .GroupBy(co => new { co.CustomerName, co.CustomerEmail })
                .Select(g => new
                {
                    CustomerName = g.Key.CustomerName,
                    CustomerEmail = g.Key.CustomerEmail,
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

            var aggregatedOrders = customerSpending.Select(cs => new CustomerTotalAmount
            {
                CustomerName = cs.CustomerName,
                CustomerEmail = cs.CustomerEmail,
                TotalAmount = cs.TotalAmount
            }).ToList();

            CustomerOrdersListView.ItemsSource = aggregatedOrders;
        }

        private async Task LoadProductReviewChart()
        {
            var productReviews = await _databaseService.GetProductReviewsAsync();
            var productRatings = productReviews
                .GroupBy(pr => pr.ProductName)
                .Select(g => new
                {
                    ProductName = g.Key,
                    AverageRating = g.Average(pr => pr.Rating)
                })
                .OrderByDescending(pr => pr.AverageRating)
                .ToList();

            var maxRating = productRatings.Max(pr => pr.AverageRating);

            ProductReviewChart.Children.Clear();

            foreach (var pr in productRatings)
            {
                var bar = new BoxView
                {
                    HeightRequest = 20,
                    WidthRequest = (double)(pr.AverageRating / maxRating) * 300,
                    Color = Colors.Green,
                    HorizontalOptions = LayoutOptions.Start
                };

                var label = new Label
                {
                    Text = $"{pr.ProductName}: {pr.AverageRating:F1}",
                    VerticalOptions = LayoutOptions.Center
                };

                var stack = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    Children = { bar, label }
                };

                ProductReviewChart.Children.Add(stack);
            }

            ProductReviewsListView.ItemsSource = productReviews;
        }

        private async Task LoadOrderItemsChart()
        {
            var orderItems = await _databaseService.GetOrderDetailsAsync();
            var productCounts = orderItems
                .GroupBy(oi => oi.ProductName)
                .Select(g => new
                {
                    ProductName = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(pc => pc.Count)
                .ToList();

            var maxCount = productCounts.Max(pc => pc.Count);

            OrderItemsChart.Children.Clear();

            foreach (var pc in productCounts)
            {
                var bar = new BoxView
                {
                    HeightRequest = 20,
                    WidthRequest = maxCount > 0 ? (double)(pc.Count / (double)maxCount) * 300 : 0,
                    Color = Colors.Red,
                    HorizontalOptions = LayoutOptions.Start
                };

                var label = new Label
                {
                    Text = $"{pc.ProductName}: {pc.Count} times",
                    VerticalOptions = LayoutOptions.Center
                };

                var stack = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    Children = { bar, label }
                };

                OrderItemsChart.Children.Add(stack);
            }

            OrderDetailsListView.ItemsSource = orderItems;
        }
    }
}