using MauiApp1.Models;
using MauiApp1.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Timer = System.Timers.Timer;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Maui.Controls;

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
        public ICommand SetTimerCommand { get; }

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
            SetTimerCommand = new Command<TodoItem>(async (item) => await SetTimerAsync(item));
            
            LoadTodoItems();
        }

        private async void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {

            foreach (var item in TodoItems.Where(x => x.IsTimerRunning))
            {
                if (item.TimerStartedAt.HasValue)
                {
                    if (item.IsCountdown)
                    {
                        // Geri sayım modu
                        var elapsed = DateTime.Now - item.TimerStartedAt.Value;
                        item.RemainingTime = item.TargetDuration - elapsed;
                        
                        if (item.RemainingTime <= TimeSpan.Zero)
                        {
                            item.IsTimerRunning = false;
                            item.RemainingTime = TimeSpan.Zero;
                            await MainThread.InvokeOnMainThreadAsync(async () =>
                            {
                                await Shell.Current.DisplayAlert("Süre Doldu!", $"{item.Title} için süre doldu!", "Tamam");
                            });
                            break;
                        }
                    }
                    else
                    {
                        // Normal kronometre modu
                        item.ElapsedTime = DateTime.Now - item.TimerStartedAt.Value;
                    }
                    await _databaseService.SaveTodoItemAsync(item);
                }
                LoadTodoItems();
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

        private async Task SetTimerAsync(TodoItem item)
        {
            string[] durations = { "5 dakika", "15 dakika", "25 dakika", "Özel" };
            string result = await Shell.Current.DisplayActionSheet("Süre Seç", "İptal", null, durations);

            TimeSpan duration;
            switch (result)
            {
                case "5 dakika":
                    duration = TimeSpan.FromMinutes(5);
                    break;
                case "15 dakika":
                    duration = TimeSpan.FromMinutes(15);
                    break;
                case "25 dakika":
                    duration = TimeSpan.FromMinutes(25);
                    break;
                case "Özel":
                    string customMinutes = await Shell.Current.DisplayPromptAsync("Özel Süre", "Dakika giriniz:", keyboard: Keyboard.Numeric);
                    if (int.TryParse(customMinutes, out int minutes))
                    {
                        duration = TimeSpan.FromMinutes(minutes);
                    }
                    else return;
                    break;
                default:
                    return;
            }

            item.TargetDuration = duration;
            item.RemainingTime = duration;
            item.IsCountdown = true;
            await _databaseService.SaveTodoItemAsync(item);
        }
    }
} 