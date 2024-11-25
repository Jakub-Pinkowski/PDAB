using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;
using MauiApp1.Models;

namespace MauiApp1.Services
{
    public class DatabaseService
    {
        private readonly SQLiteAsyncConnection _database;

        public DatabaseService(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
        }

        public Task CreateTableAsync<T>() where T : new()
        {
            return _database.CreateTableAsync<T>();
        }

        public Task<List<T>> GetItemsAsync<T>() where T : new()
        {
            return _database.Table<T>().ToListAsync();
        }

        public Task<int> SaveItemAsync<T>(T item) where T : new()
        {
            var propertyInfo = item?.GetType().GetProperty("Id");
            if (propertyInfo != null)
            {
                var value = propertyInfo.GetValue(item);
                if (value != null && (int)value != 0)
                {
                    return _database.UpdateAsync(item);
                }
            }
            return _database.InsertAsync(item);
        }

        public Task<int> DeleteItemAsync<T>(T item) where T : new()
        {
            return _database.DeleteAsync(item);
        }

        public async Task DropAllTablesAsync()
        {
            await _database.DropTableAsync<Product>();
            // Add other tables here if you have more models
        }

        public async Task RecreateAllTablesAsync()
        {
            await CreateTableAsync<Product>();
            // Add other tables here if you have more models
        }

        public async Task ResetDatabaseAsync()
        {
            await DropAllTablesAsync();
            await RecreateAllTablesAsync();
        }
    }
}