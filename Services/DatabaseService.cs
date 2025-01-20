using SQLite;
using MauiApp1.Models;

namespace MauiApp1.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection _database;
        private const string DbName = "todo.db3";

        public DatabaseService()
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, DbName);
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<TodoItem>().Wait();
        }

        public async Task<List<TodoItem>> GetTodoItemsAsync()
        {
            return await _database.Table<TodoItem>().ToListAsync();
        }

        public async Task<TodoItem> GetTodoItemAsync(int id)
        {
            return await _database.Table<TodoItem>().Where(i => i.Id == id).FirstOrDefaultAsync();
        }

        public async Task<int> SaveTodoItemAsync(TodoItem item)
        {
            if (item.Id != 0)
            {
                return await _database.UpdateAsync(item);
            }
            return await _database.InsertAsync(item);
        }

        public async Task<int> DeleteTodoItemAsync(TodoItem item)
        {
            return await _database.DeleteAsync(item);
        }
    }
} 