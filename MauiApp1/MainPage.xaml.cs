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
            LoadCustomerOrders();
        }

        private async void OnResetDatabaseClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Confirm Reset", "Are you sure you want to reset the database? This action cannot be undone.", "Yes", "No");
            if (confirm)
            {
                await _databaseService.ResetDatabaseAsync();
                await DisplayAlert("Success", "Database has been reset.", "OK");
                // Reload the data after resetting the database
                LoadCustomerOrders();
            }
        }

        private async void LoadCustomerOrders()
        {
            var customerOrders = await _databaseService.GetCustomerOrdersAsync();
            CustomerOrdersListView.ItemsSource = customerOrders;
        }
    }
}