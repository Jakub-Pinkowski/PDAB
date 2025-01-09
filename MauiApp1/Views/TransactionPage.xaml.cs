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
    public partial class TransactionPage : ContentPage, INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private Transaction? _editingTransaction;
        private string _buttonText = "Add Transaction";
        private bool _isEditing = false;
        private bool _isSortedAscending = true;
        private List<Transaction> _masterTransactionList = new List<Transaction>();

        public new event PropertyChangedEventHandler? PropertyChanged;

        public TransactionPage(DatabaseService databaseService)
        {
            InitializeComponent();
            BindingContext = this;
            _databaseService = databaseService;
            LoadTransactionsAsync();
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

        private async void LoadTransactionsAsync()
        {
            _masterTransactionList = await _databaseService.GetItemsAsync<Transaction>();
            TransactionsCollectionView.ItemsSource = _masterTransactionList;
        }

        private async void OnAddTransactionClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(OrderIdEntry.Text) || !int.TryParse(OrderIdEntry.Text, out var orderId) || orderId <= 0 ||
                string.IsNullOrWhiteSpace(TransactionDateEntry.Text) || !DateTime.TryParse(TransactionDateEntry.Text, out var transactionDate) ||
                string.IsNullOrWhiteSpace(AmountEntry.Text) || !decimal.TryParse(AmountEntry.Text, out var amount) || amount <= 0)
            {
                await DisplayAlert("Validation Error", "Please ensure all fields are filled correctly. Order ID must be a positive integer, Transaction Date must be a valid date, and Amount must be a positive decimal.", "OK");
                return;
            }

            if (_editingTransaction == null)
            {
                var newTransaction = new Transaction
                {
                    OrderId = orderId,
                    TransactionDate = transactionDate,
                    Amount = amount
                };

                await _databaseService.SaveItemAsync(newTransaction);
            }
            else
            {
                _editingTransaction.OrderId = orderId;
                _editingTransaction.TransactionDate = transactionDate;
                _editingTransaction.Amount = amount;
                await _databaseService.SaveItemAsync(_editingTransaction);
                _editingTransaction = null;
                ButtonText = "Add Transaction";
                IsEditing = false;
            }

            LoadTransactionsAsync();

            // Reset the input fields
            OrderIdEntry.Text = string.Empty;
            TransactionDateEntry.Text = string.Empty;
            AmountEntry.Text = string.Empty;
        }

        private async void OnDeleteTransactionClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var transaction = button?.CommandParameter as Transaction;

            if (transaction != null)
            {
                bool confirm = await DisplayAlert("Confirm Delete", $"Are you sure you want to delete the transaction with Order ID {transaction.OrderId}?", "Yes", "No");
                if (confirm)
                {
                    await _databaseService.DeleteItemAsync(transaction);
                    LoadTransactionsAsync();
                }
            }
        }

        private void OnEditTransactionClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var transaction = button?.CommandParameter as Transaction;

            if (transaction != null)
            {
                _editingTransaction = transaction;
                OrderIdEntry.Text = transaction.OrderId.ToString();
                TransactionDateEntry.Text = transaction.TransactionDate.ToString("yyyy-MM-dd");
                AmountEntry.Text = transaction.Amount.ToString();
                ButtonText = "Edit Transaction";
                IsEditing = true;
            }
        }

        private void OnCancelEditClicked(object sender, EventArgs e)
        {
            _editingTransaction = null;
            ButtonText = "Add Transaction";
            IsEditing = false;

            // Reset the input fields
            OrderIdEntry.Text = string.Empty;
            TransactionDateEntry.Text = string.Empty;
            AmountEntry.Text = string.Empty;
        }

        private void OnRefreshClicked(object sender, EventArgs e)
        {
            LoadTransactionsAsync();
        }

        private void SortTransactions(string criterion)
        {
            var transactions = TransactionsCollectionView.ItemsSource.Cast<Transaction>().ToList();
            switch (criterion)
            {
                case "OrderId":
                    transactions = _isSortedAscending ? transactions.OrderBy(t => t.OrderId).ToList() : transactions.OrderByDescending(t => t.OrderId).ToList();
                    _isSortedAscending = !_isSortedAscending;
                    break;
                case "TransactionDate":
                    transactions = _isSortedAscending ? transactions.OrderBy(t => t.TransactionDate).ToList() : transactions.OrderByDescending(t => t.TransactionDate).ToList();
                    _isSortedAscending = !_isSortedAscending;
                    break;
                case "Amount":
                    transactions = _isSortedAscending ? transactions.OrderBy(t => t.Amount).ToList() : transactions.OrderByDescending(t => t.Amount).ToList();
                    _isSortedAscending = !_isSortedAscending;
                    break;
            }
            TransactionsCollectionView.ItemsSource = transactions;
        }

        private void OnSortByOrderIdClicked(object sender, EventArgs e)
        {
            SortTransactions("OrderId");
        }

        private void OnSortByTransactionDateClicked(object sender, EventArgs e)
        {
            SortTransactions("TransactionDate");
        }

        private void OnSortByAmountClicked(object sender, EventArgs e)
        {
            SortTransactions("Amount");
        }

        private void FilterTransactions(string criterion, string minValue, string maxValue)
        {
            var transactions = _masterTransactionList;
            switch (criterion)
            {
                case "OrderId":
                    if (int.TryParse(minValue, out int minOrderId) && int.TryParse(maxValue, out int maxOrderId))
                    {
                        transactions = transactions.Where(t => t.OrderId >= minOrderId && t.OrderId <= maxOrderId).ToList();
                    }
                    else if (int.TryParse(minValue, out minOrderId))
                    {
                        transactions = transactions.Where(t => t.OrderId >= minOrderId).ToList();
                    }
                    else if (int.TryParse(maxValue, out maxOrderId))
                    {
                        transactions = transactions.Where(t => t.OrderId <= maxOrderId).ToList();
                    }
                    break;
                case "TransactionDate":
                    if (DateTime.TryParse(minValue, out DateTime minTransactionDate) && DateTime.TryParse(maxValue, out DateTime maxTransactionDate))
                    {
                        transactions = transactions.Where(t => t.TransactionDate.Date >= minTransactionDate.Date && t.TransactionDate.Date <= maxTransactionDate.Date).ToList();
                    }
                    else if (DateTime.TryParse(minValue, out minTransactionDate))
                    {
                        transactions = transactions.Where(t => t.TransactionDate.Date >= minTransactionDate.Date).ToList();
                    }
                    else if (DateTime.TryParse(maxValue, out maxTransactionDate))
                    {
                        transactions = transactions.Where(t => t.TransactionDate.Date <= maxTransactionDate.Date).ToList();
                    }
                    break;
                case "Amount":
                    if (decimal.TryParse(minValue, out decimal minAmount) && decimal.TryParse(maxValue, out decimal maxAmount))
                    {
                        transactions = transactions.Where(t => t.Amount >= minAmount && t.Amount <= maxAmount).ToList();
                    }
                    else if (decimal.TryParse(minValue, out minAmount))
                    {
                        transactions = transactions.Where(t => t.Amount >= minAmount).ToList();
                    }
                    else if (decimal.TryParse(maxValue, out maxAmount))
                    {
                        transactions = transactions.Where(t => t.Amount <= maxAmount).ToList();
                    }
                    break;
            }
            TransactionsCollectionView.ItemsSource = transactions;
        }

        private void OnFilterByOrderIdClicked(object sender, EventArgs e)
        {
            FilterTransactions("OrderId", MinOrderIdEntry.Text, MaxOrderIdEntry.Text);
        }

        private void OnFilterByTransactionDateClicked(object sender, EventArgs e)
        {
            FilterTransactions("TransactionDate", MinTransactionDateEntry.Text, MaxTransactionDateEntry.Text);
        }

        private void OnFilterByAmountClicked(object sender, EventArgs e)
        {
            FilterTransactions("Amount", MinAmountEntry.Text, MaxAmountEntry.Text);
        }

        private void OnRefreshFiltersClicked(object sender, EventArgs e)
        {
            // Clear all filter inputs
            MinOrderIdEntry.Text = string.Empty;
            MaxOrderIdEntry.Text = string.Empty;
            MinTransactionDateEntry.Text = string.Empty;
            MaxTransactionDateEntry.Text = string.Empty;
            MinAmountEntry.Text = string.Empty;
            MaxAmountEntry.Text = string.Empty;

            // Reset the displayed transactions to the full list
            TransactionsCollectionView.ItemsSource = _masterTransactionList;
        }

        protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}