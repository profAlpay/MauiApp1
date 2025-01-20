using MauiApp1.Models;
using MauiApp1.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MauiApp1.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private ObservableCollection<TodoItem> _todoItems;
        
        public ObservableCollection<TodoItem> TodoItems
        {
            get => _todoItems;
            set => SetProperty(ref _todoItems, value);
        }

        public ICommand AddTodoCommand { get; }
        public ICommand ToggleCompletedCommand { get; }
        public ICommand DeleteTodoCommand { get; }

        public MainViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            TodoItems = new ObservableCollection<TodoItem>();
            
            AddTodoCommand = new Command(async () => await AddTodoAsync());
            ToggleCompletedCommand = new Command<TodoItem>(async (item) => await ToggleCompletedAsync(item));
            DeleteTodoCommand = new Command<TodoItem>(async (item) => await DeleteTodoAsync(item));
            
            LoadTodoItems();
        }

        private async void LoadTodoItems()
        {
            var items = await _databaseService.GetTodoItemsAsync();
            TodoItems = new ObservableCollection<TodoItem>(items);
        }

        private async Task AddTodoAsync()
        {
            string result = await Shell.Current.DisplayPromptAsync("Yeni Görev", "Görev başlığını girin:");
            if (!string.IsNullOrWhiteSpace(result))
            {
                var newItem = new TodoItem 
                { 
                    Title = result,
                    CreatedAt = DateTime.Now 
                };
                
                await _databaseService.SaveTodoItemAsync(newItem);
                TodoItems.Add(newItem);
            }
        }

        private async Task ToggleCompletedAsync(TodoItem item)
        {
            item.IsCompleted = !item.IsCompleted;
            item.CompletedAt = item.IsCompleted ? DateTime.Now : null;
            await _databaseService.SaveTodoItemAsync(item);
        }

        private async Task DeleteTodoAsync(TodoItem item)
        {
            await _databaseService.DeleteTodoItemAsync(item);
            TodoItems.Remove(item);
        }
    }
} 