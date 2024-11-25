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
    public partial class TransactionPage : ContentPage, INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private Transaction? _editingTransaction;
        private string _buttonText = "Add Transaction";
        private bool _isEditing = false;

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
            var transactions = await _databaseService.GetItemsAsync<Transaction>();
            TransactionsCollectionView.ItemsSource = transactions;
        }

        private async void OnAddTransactionClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(OrderIdEntry.Text) || !int.TryParse(OrderIdEntry.Text, out var orderId) || orderId <= 0 ||
                string.IsNullOrWhiteSpace(TransactionDateEntry.Text) || !DateTime.TryParse(TransactionDateEntry.Text, out var transactionDate) ||
                string.IsNullOrWhiteSpace(AmountEntry.Text) || !decimal.TryParse(AmountEntry.Text, out var amount) || amount <= 0)
            {
                await DisplayAlert("Validation Error", "Please ensure all fields are filled correctly. Order ID must be a positive integer, Transaction Date must be a valid date, and Amount must be a positive number.", "OK");
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
                bool confirm = await DisplayAlert("Confirm Delete", $"Are you sure you want to delete the transaction with ID {transaction.Id}?", "Yes", "No");
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

        protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}