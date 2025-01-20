using MauiApp1.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MauiApp1.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private ObservableCollection<TodoItem> _todoItems;
        public ObservableCollection<TodoItem> TodoItems
        {
            get => _todoItems;
            set => SetProperty(ref _todoItems, value);
        }

        private ICommand _addTodoCommand;
        public ICommand AddTodoCommand => _addTodoCommand ??= new Command(async () => await AddTodoAsync());

        public MainViewModel()
        {
            TodoItems = new ObservableCollection<TodoItem>
            {
                // Test için örnek veriler
                new TodoItem { Title = "Alışveriş yapılacak", IsCompleted = false },
                new TodoItem { Title = "Spor yapılacak", IsCompleted = true },
                new TodoItem { Title = "Kitap okunacak", IsCompleted = false }
            };
        }

        private async Task AddTodoAsync()
        {
            string result = await Shell.Current.DisplayPromptAsync("Yeni Görev", "Görev başlığını girin:");
            if (!string.IsNullOrWhiteSpace(result))
            {
                TodoItems.Add(new TodoItem { Title = result, IsCompleted = false });
            }
        }
    }
} 