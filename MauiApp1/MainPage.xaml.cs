﻿using MauiApp1.Models;
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
        }

        private async void OnResetDatabaseClicked(object sender, EventArgs e)
        {
            await _databaseService.ResetDatabaseAsync();
            await DisplayAlert("Success", "Database has been reset.", "OK");
        }
    }
}