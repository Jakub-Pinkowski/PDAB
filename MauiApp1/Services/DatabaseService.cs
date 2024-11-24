using SQLite;
using MauiApp1.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MauiApp1.Services
{
    public class DatabaseService
    {
        private readonly SQLiteAsyncConnection _database;

        public DatabaseService(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<Customer>().Wait();
        }

        public Task<List<Customer>> GetCustomersAsync()
        {
            return _database.Table<Customer>().ToListAsync();
        }

        public Task<int> SaveCustomerAsync(Customer customer)
        {
            if (customer.Id != 0)
            {
                return _database.UpdateAsync(customer);
            }
            else
            {
                return _database.InsertAsync(customer);
            }
        }

        public Task<int> DeleteCustomerAsync(Customer customer)
        {
            return _database.DeleteAsync(customer);
        }
    }
}