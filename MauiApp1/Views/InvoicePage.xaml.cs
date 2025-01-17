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
    public partial class InvoicePage : ContentPage, INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private Invoice? _editingInvoice;
        private string _buttonText = "Add Invoice";
        private bool _isEditing = false;
        private bool _isSortedAscending = true;
        private List<Invoice> _masterInvoiceList = new List<Invoice>();

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
            _masterInvoiceList = await _databaseService.GetItemsAsync<Invoice>();
            InvoicesCollectionView.ItemsSource = _masterInvoiceList;
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

        private void SortInvoices(string criterion)
        {
            var invoices = InvoicesCollectionView.ItemsSource.Cast<Invoice>().ToList();
            switch (criterion)
            {
                case "OrderId":
                    invoices = _isSortedAscending ? invoices.OrderBy(i => i.OrderId).ToList() : invoices.OrderByDescending(i => i.OrderId).ToList();
                    _isSortedAscending = !_isSortedAscending;
                    break;
                case "InvoiceDate":
                    invoices = _isSortedAscending ? invoices.OrderBy(i => i.InvoiceDate).ToList() : invoices.OrderByDescending(i => i.InvoiceDate).ToList();
                    _isSortedAscending = !_isSortedAscending;
                    break;
                case "TotalAmount":
                    invoices = _isSortedAscending ? invoices.OrderBy(i => i.TotalAmount).ToList() : invoices.OrderByDescending(i => i.TotalAmount).ToList();
                    _isSortedAscending = !_isSortedAscending;
                    break;
            }
            InvoicesCollectionView.ItemsSource = invoices;
        }

        private void OnSortByOrderIdClicked(object sender, EventArgs e)
        {
            SortInvoices("OrderId");
        }

        private void OnSortByInvoiceDateClicked(object sender, EventArgs e)
        {
            SortInvoices("InvoiceDate");
        }

        private void OnSortByTotalAmountClicked(object sender, EventArgs e)
        {
            SortInvoices("TotalAmount");
        }

        protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void FilterInvoices(string criterion, string minValue, string maxValue)
        {
            var invoices = _masterInvoiceList;
            switch (criterion)
            {
                case "OrderId":
                    if (int.TryParse(minValue, out int minOrderId) && int.TryParse(maxValue, out int maxOrderId))
                    {
                        invoices = invoices.Where(i => i.OrderId >= minOrderId && i.OrderId <= maxOrderId).ToList();
                    }
                    else if (int.TryParse(minValue, out minOrderId))
                    {
                        invoices = invoices.Where(i => i.OrderId >= minOrderId).ToList();
                    }
                    else if (int.TryParse(maxValue, out maxOrderId))
                    {
                        invoices = invoices.Where(i => i.OrderId <= maxOrderId).ToList();
                    }
                    break;
                case "InvoiceDate":
                    if (DateTime.TryParse(minValue, out DateTime minInvoiceDate) && DateTime.TryParse(maxValue, out DateTime maxInvoiceDate))
                    {
                        invoices = invoices.Where(i => i.InvoiceDate.Date >= minInvoiceDate.Date && i.InvoiceDate.Date <= maxInvoiceDate.Date).ToList();
                    }
                    else if (DateTime.TryParse(minValue, out minInvoiceDate))
                    {
                        invoices = invoices.Where(i => i.InvoiceDate.Date >= minInvoiceDate.Date).ToList();
                    }
                    else if (DateTime.TryParse(maxValue, out maxInvoiceDate))
                    {
                        invoices = invoices.Where(i => i.InvoiceDate.Date <= maxInvoiceDate.Date).ToList();
                    }
                    break;
                case "TotalAmount":
                    if (decimal.TryParse(minValue, out decimal minTotalAmount) && decimal.TryParse(maxValue, out decimal maxTotalAmount))
                    {
                        invoices = invoices.Where(i => i.TotalAmount >= minTotalAmount && i.TotalAmount <= maxTotalAmount).ToList();
                    }
                    else if (decimal.TryParse(minValue, out minTotalAmount))
                    {
                        invoices = invoices.Where(i => i.TotalAmount >= minTotalAmount).ToList();
                    }
                    else if (decimal.TryParse(maxValue, out maxTotalAmount))
                    {
                        invoices = invoices.Where(i => i.TotalAmount <= maxTotalAmount).ToList();
                    }
                    break;
            }
            InvoicesCollectionView.ItemsSource = invoices;
        }

        private void OnFilterByOrderIdClicked(object sender, EventArgs e)
        {
            FilterInvoices("OrderId", MinOrderIdEntry.Text, MaxOrderIdEntry.Text);
        }

        private void OnFilterByInvoiceDateClicked(object sender, EventArgs e)
        {
            FilterInvoices("InvoiceDate", MinInvoiceDateEntry.Text, MaxInvoiceDateEntry.Text);
        }

        private void OnFilterByTotalAmountClicked(object sender, EventArgs e)
        {
            FilterInvoices("TotalAmount", MinTotalAmountEntry.Text, MaxTotalAmountEntry.Text);
        }

        private void OnRefreshFiltersClicked(object sender, EventArgs e)
        {
            // Clear all filter inputs
            MinOrderIdEntry.Text = string.Empty;
            MaxOrderIdEntry.Text = string.Empty;
            MinInvoiceDateEntry.Text = string.Empty;
            MaxInvoiceDateEntry.Text = string.Empty;
            MinTotalAmountEntry.Text = string.Empty;
            MaxTotalAmountEntry.Text = string.Empty;

            // Reset the displayed invoices to the full list
            InvoicesCollectionView.ItemsSource = _masterInvoiceList;
        }
    }
}