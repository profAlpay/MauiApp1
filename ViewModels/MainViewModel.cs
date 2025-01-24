using MauiApp1.Models;
using MauiApp1.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Timer = System.Timers.Timer;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Maui.Controls;
using MauiApp1.Views;

namespace MauiApp1.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly Timer _timer;
        private ObservableCollection<TodoItem> _todoItems;
        private string _displayTime = "00:00:00";
        private TimeSpan _chronoTime = TimeSpan.Zero;
        private TimeSpan _countdownTime = TimeSpan.FromMinutes(25); // Varsayılan 25 dakika
        private bool _isRunning;
        private bool _isChronometer = true;
        private bool _isTimer;
        private bool _isBreakTime;
        private TimeSpan _breakDuration = TimeSpan.FromMinutes(5); // Varsayılan mola süresi
        
        public ObservableCollection<TodoItem> TodoItems
        {
            get => _todoItems;
            set => SetProperty(ref _todoItems, value);
        }

        public string DisplayTime
        {
            get => _displayTime;
            set => SetProperty(ref _displayTime, value);
        }

        public bool IsRunning
        {
            get => _isRunning;
            set => SetProperty(ref _isRunning, value);
        }

        public bool IsChronometer
        {
            get => _isChronometer;
            set
            {
                if (SetProperty(ref _isChronometer, value) && value)
                {
                    IsTimer = false;
                }
            }
        }

        public bool IsTimer
        {
            get => _isTimer;
            set
            {
                if (SetProperty(ref _isTimer, value) && value)
                {
                    IsChronometer = false;
                }
            }
        }

        public bool IsBreakTime
        {
            get => _isBreakTime;
            set => SetProperty(ref _isBreakTime, value);
        }

        public ICommand AddTodoCommand { get; }
        public ICommand ToggleCompletedCommand { get; }
        public ICommand DeleteTodoCommand { get; }
        public ICommand ToggleTimerCommand { get; }
        public ICommand SetTimerCommand { get; }
        public ICommand SetTimerModeCommand { get; }
        public ICommand StartStopCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand SetCountdownTimeCommand { get; }
        public ICommand ShowTaskDetailCommand { get; }

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

            StartStopCommand = new Command(StartStop);
            ResetCommand = new Command(Reset);
            SetCountdownTimeCommand = new Command(async () => await SetCountdownTimeAsync());

            SetTimerModeCommand = new Command<string>(mode =>
            {
                switch (mode)
                {
                    case "Chronometer":
                        IsChronometer = true;
                        DisplayTime = _chronoTime.ToString(@"hh\:mm\:ss");
                        IsRunning = false;
                        break;
                    case "Timer":
                        IsTimer = true;
                        DisplayTime = _countdownTime.ToString(@"mm\:ss");
                        IsRunning = false;
                        break;
                }
            });

            // Timer'ı her saniye güncelle
            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                if (IsRunning)
                {
                    if (IsChronometer)
                    {
                        _chronoTime = _chronoTime.Add(TimeSpan.FromSeconds(1));
                        DisplayTime = _chronoTime.ToString(@"hh\:mm\:ss");
                    }
                    else if (IsTimer)
                    {
                        _countdownTime = _countdownTime.Subtract(TimeSpan.FromSeconds(1));
                        DisplayTime = _countdownTime.ToString(@"mm\:ss");

                        if (_countdownTime <= TimeSpan.Zero)
                        {
                            IsRunning = false;
                            MainThread.BeginInvokeOnMainThread(async () =>
                            {
                                await Shell.Current.DisplayAlert("Süre Doldu!", "Geri sayım tamamlandı!", "Tamam");
                            });
                            Reset();
                        }
                    }
                }
                return true;
            });

            ShowTaskDetailCommand = new Command<TodoItem>(async (item) => await ShowTaskDetail(item));
        }

        private async void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            foreach (var item in TodoItems.Where(x => x.IsTimerRunning))
            {
                if (item.IsCountdown)
                {
                    var elapsed = DateTime.Now - item.TimerStartedAt.Value;
                    item.RemainingTime = item.TargetDuration - elapsed;
                    
                    if (item.RemainingTime <= TimeSpan.Zero)
                    {
                        item.IsTimerRunning = false;
                        item.RemainingTime = TimeSpan.Zero;

                        await MainThread.InvokeOnMainThreadAsync(async () =>
                        {
                            if (!IsBreakTime)
                            {
                                var startBreak = await Shell.Current.DisplayAlert(
                                    "Süre Doldu!", 
                                    $"{item.Title} için çalışma süresi doldu! Mola başlasın mı?", 
                                    "Evet", "Hayır");

                                if (startBreak)
                                {
                                    IsBreakTime = true;
                                    item.TargetDuration = _breakDuration;
                                    item.RemainingTime = _breakDuration;
                                    item.TimerStartedAt = DateTime.Now;
                                    item.IsTimerRunning = true;
                                }
                            }
                            else
                            {
                                await Shell.Current.DisplayAlert(
                                    "Mola Bitti!", 
                                    "Mola süreniz doldu. Yeni bir pomodoro başlatabilirsiniz.", 
                                    "Tamam");
                                IsBreakTime = false;
                            }
                        });
                    }
                    await _databaseService.SaveTodoItemAsync(item);
                }
                else
                {
                    // Normal kronometre modu
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
            string title = await Shell.Current.DisplayPromptAsync("Yeni Görev", "Görev başlığını girin:");
            if (!string.IsNullOrWhiteSpace(title))
            {
                string description = await Shell.Current.DisplayPromptAsync("Görev Detayı", "Görev açıklamasını girin:");
                
                var newItem = new TodoItem 
                { 
                    Title = title,
                    Description = description ?? "",  // Açıklama boş bırakılabilir
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
            var options = new string[] { 
                "25 dk + 5 dk mola", 
                "45 dk + 15 dk mola",
                "50 dk + 10 dk mola",
                "Özel" 
            };

            string result = await Shell.Current.DisplayActionSheet("Pomodoro Seç", "İptal", null, options);

            TimeSpan workDuration;
            switch (result)
            {
                case "25 dk + 5 dk mola":
                    workDuration = TimeSpan.FromMinutes(25);
                    _breakDuration = TimeSpan.FromMinutes(5);
                    break;
                case "45 dk + 15 dk mola":
                    workDuration = TimeSpan.FromMinutes(45);
                    _breakDuration = TimeSpan.FromMinutes(15);
                    break;
                case "50 dk + 10 dk mola":
                    workDuration = TimeSpan.FromMinutes(50);
                    _breakDuration = TimeSpan.FromMinutes(10);
                    break;
                case "Özel":
                    string workMinutes = await Shell.Current.DisplayPromptAsync(
                        "Çalışma Süresi", 
                        "Dakika giriniz:",
                        keyboard: Keyboard.Numeric);
                    
                    string breakMinutes = await Shell.Current.DisplayPromptAsync(
                        "Mola Süresi", 
                        "Dakika giriniz:",
                        keyboard: Keyboard.Numeric);

                    if (int.TryParse(workMinutes, out int work) && 
                        int.TryParse(breakMinutes, out int brk))
                    {
                        workDuration = TimeSpan.FromMinutes(work);
                        _breakDuration = TimeSpan.FromMinutes(brk);
                    }
                    else return;
                    break;
                default:
                    return;
            }

            item.TargetDuration = workDuration;
            item.RemainingTime = workDuration;
            item.IsCountdown = true;
            IsBreakTime = false;
            await _databaseService.SaveTodoItemAsync(item);
        }

        private void StartStop()
        {
            IsRunning = !IsRunning;
        }

        private void Reset()
        {
            IsRunning = false;
            if (IsChronometer)
            {
                _chronoTime = TimeSpan.Zero;
                DisplayTime = "00:00:00";
            }
            else if (IsTimer)
            {
                _countdownTime = TimeSpan.FromMinutes(25); // Varsayılan süreye dön
                DisplayTime = _countdownTime.ToString(@"mm\:ss");
            }
        }

        private async Task SetCountdownTimeAsync()
        {
            if (IsRunning) return;

            string result = await Shell.Current.DisplayPromptAsync(
                "Süre Ayarla",
                "Dakika giriniz:",
                keyboard: Keyboard.Numeric);

            if (int.TryParse(result, out int minutes) && minutes > 0)
            {
                _countdownTime = TimeSpan.FromMinutes(minutes);
                DisplayTime = _countdownTime.ToString(@"mm\:ss");
            }
        }

        private async Task ShowTaskDetail(TodoItem item)
        {
            var detailViewModel = new TaskDetailViewModel(_databaseService) { Task = item };
            var detailPage = new TaskDetailPage { BindingContext = detailViewModel };
            await Shell.Current.Navigation.PushAsync(detailPage);
        }
    }
} 