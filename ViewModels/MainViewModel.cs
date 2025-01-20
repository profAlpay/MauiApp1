using MauiApp1.Models;
using MauiApp1.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Timer = System.Timers.Timer;

namespace MauiApp1.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly Timer _timer;
        private ObservableCollection<TodoItem> _todoItems;
        
        public ObservableCollection<TodoItem> TodoItems
        {
            get => _todoItems;
            set => SetProperty(ref _todoItems, value);
        }

        public ICommand AddTodoCommand { get; }
        public ICommand ToggleCompletedCommand { get; }
        public ICommand DeleteTodoCommand { get; }
        public ICommand ToggleTimerCommand { get; }

        public MainViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            TodoItems = new ObservableCollection<TodoItem>();
            
            // Timer'ı başlat (her saniye güncellenecek)
            _timer = new Timer(1000);
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();

            AddTodoCommand = new Command(async () => await AddTodoAsync());
            ToggleCompletedCommand = new Command<TodoItem>(async (item) => await ToggleCompletedAsync(item));
            DeleteTodoCommand = new Command<TodoItem>(async (item) => await DeleteTodoAsync(item));
            ToggleTimerCommand = new Command<TodoItem>(async (item) => await ToggleTimerAsync(item));
            
            LoadTodoItems();
        }

        private async void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            foreach (var item in TodoItems.Where(x => x.IsTimerRunning))
            {
                if (item.TimerStartedAt.HasValue)
                {
                    item.ElapsedTime = DateTime.Now - item.TimerStartedAt.Value;
                    await _databaseService.SaveTodoItemAsync(item);
                }
            }
        }

        private async Task ToggleTimerAsync(TodoItem item)
        {
            if (item.IsTimerRunning)
            {
                // Timer'ı durdur
                item.IsTimerRunning = false;
                item.TimerStartedAt = null;
            }
            else
            {
                // Timer'ı başlat
                item.IsTimerRunning = true;
                item.TimerStartedAt = DateTime.Now - item.ElapsedTime;
            }
            await _databaseService.SaveTodoItemAsync(item);
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