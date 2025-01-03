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
    public partial class InvoicePage : ContentPage, INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private Invoice? _editingInvoice;
        private string _buttonText = "Add Invoice";
        private bool _isEditing = false;

        public new event PropertyChangedEventHandler? PropertyChanged;

        public InvoicePage(DatabaseService databaseService)
        {
            InitializeComponent();
            BindingContext = this;
            _databaseService = databaseService;
            LoadInvoicesAsync();
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

        private async void LoadInvoicesAsync()
        {
            var invoices = await _databaseService.GetItemsAsync<Invoice>();
            InvoicesCollectionView.ItemsSource = invoices;
        }

        private async void OnAddInvoiceClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(OrderIdEntry.Text) || !int.TryParse(OrderIdEntry.Text, out var orderId) || orderId <= 0 ||
                string.IsNullOrWhiteSpace(InvoiceDateEntry.Text) || !DateTime.TryParse(InvoiceDateEntry.Text, out var invoiceDate) ||
                string.IsNullOrWhiteSpace(TotalAmountEntry.Text) || !decimal.TryParse(TotalAmountEntry.Text, out var totalAmount) || totalAmount <= 0)
            {
                await DisplayAlert("Validation Error", "Please ensure all fields are filled correctly. Order ID must be a positive integer, Invoice Date must be a valid date, and Total Amount must be a positive number.", "OK");
                return;
            }

            if (_editingInvoice == null)
            {
                var newInvoice = new Invoice
                {
                    OrderId = orderId,
                    InvoiceDate = invoiceDate,
                    TotalAmount = totalAmount
                };

                await _databaseService.SaveItemAsync(newInvoice);
            }
            else
            {
                _editingInvoice.OrderId = orderId;
                _editingInvoice.InvoiceDate = invoiceDate;
                _editingInvoice.TotalAmount = totalAmount;
                await _databaseService.SaveItemAsync(_editingInvoice);
                _editingInvoice = null;
                ButtonText = "Add Invoice";
                IsEditing = false;
            }

            LoadInvoicesAsync();

            // Reset the input fields
            OrderIdEntry.Text = string.Empty;
            InvoiceDateEntry.Text = string.Empty;
            TotalAmountEntry.Text = string.Empty;
        }

        private async void OnDeleteInvoiceClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var invoice = button?.CommandParameter as Invoice;

            if (invoice != null)
            {
                bool confirm = await DisplayAlert("Confirm Delete", $"Are you sure you want to delete the invoice with ID {invoice.Id}?", "Yes", "No");
                if (confirm)
                {
                    await _databaseService.DeleteItemAsync(invoice);
                    LoadInvoicesAsync();
                }
            }
        }

        private void OnEditInvoiceClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var invoice = button?.CommandParameter as Invoice;

            if (invoice != null)
            {
                _editingInvoice = invoice;
                OrderIdEntry.Text = invoice.OrderId.ToString();
                InvoiceDateEntry.Text = invoice.InvoiceDate.ToString("yyyy-MM-dd");
                TotalAmountEntry.Text = invoice.TotalAmount.ToString();
                ButtonText = "Edit Invoice";
                IsEditing = true;
            }
        }

        private void OnCancelEditClicked(object sender, EventArgs e)
        {
            _editingInvoice = null;
            ButtonText = "Add Invoice";
            IsEditing = false;

            // Reset the input fields
            OrderIdEntry.Text = string.Empty;
            InvoiceDateEntry.Text = string.Empty;
            TotalAmountEntry.Text = string.Empty;
        }

        private void OnRefreshClicked(object sender, EventArgs e)
        {
            LoadInvoicesAsync();
        }

        protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}