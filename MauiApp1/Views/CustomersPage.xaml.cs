using MauiApp1.Models;
using MauiApp1.Services;
using Microsoft.Maui.Controls;

namespace MauiApp1
{
    public partial class CustomersPage : ContentPage
    {
        private readonly DatabaseService _databaseService;

        public CustomersPage(DatabaseService databaseService)
        {
            InitializeComponent();
            _databaseService = databaseService;
            LoadCustomersAsync();
        }

        private async void LoadCustomersAsync()
        {
            var customers = await _databaseService.GetCustomersAsync();
            CustomersCollectionView.ItemsSource = customers;
        }
    }
}