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
            LoadCustomersAsync();
        }

        private async void LoadCustomersAsync()
        {
            var customers = await _databaseService.GetCustomersAsync();
            CustomersCollectionView.ItemsSource = customers;
        }
    }
}