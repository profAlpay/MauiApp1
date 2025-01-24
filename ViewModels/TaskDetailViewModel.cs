using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MauiApp1.Models;
using MauiApp1.Services;
using MauiApp1.ViewModels;
using MauiApp1.Views;
using Microsoft.Maui.Controls;

namespace MauiApp1.ViewModels
{
    public class TaskDetailViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private TodoItem _task;
        private string _videoThumbnail;
        private string _videoTitle;
        private bool _hasVideoLink;
        private bool _isEditing;
        private string _title;
        private string _description;

        public TodoItem Task
        {
            get => _task;
            set
            {
                if (SetProperty(ref _task, value))
                {
                    Title = value.Title;
                    Description = value.Description;
                    CheckForVideoLink();
                }
            }
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string Description
        {
            get => _description;
            set
            {
                if (SetProperty(ref _description, value))
                {
                    CheckForVideoLink();
                }
            }
        }

        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        public string EditButtonText => IsEditing ? "✓" : "✏️";

        public string VideoThumbnail 
        { 
            get => _videoThumbnail;
            private set => SetProperty(ref _videoThumbnail, value);
        }

        public string VideoTitle
        {
            get => _videoTitle;
            private set => SetProperty(ref _videoTitle, value);
        }

        public bool HasVideoLink
        {
            get => _hasVideoLink;
            private set => SetProperty(ref _hasVideoLink, value);
        }

        public ICommand OpenVideoCommand { get; }
        public ICommand ToggleEditCommand { get; }
        public ICommand SaveChangesCommand { get; }

        public TaskDetailViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            OpenVideoCommand = new Command(async () => await OpenVideo());
            ToggleEditCommand = new Command(ToggleEdit);
            SaveChangesCommand = new Command(async () => await SaveChanges());
        }

        private void ToggleEdit()
        {
            IsEditing = !IsEditing;
            OnPropertyChanged(nameof(EditButtonText));
        }

        private async Task SaveChanges()
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                await Shell.Current.DisplayAlert("Hata", "Başlık boş olamaz!", "Tamam");
                return;
            }

            Task.Title = Title;
            Task.Description = Description;
            await _databaseService.SaveTodoItemAsync(Task);
            IsEditing = false;
            OnPropertyChanged(nameof(EditButtonText));
            await Shell.Current.DisplayAlert("Başarılı", "Değişiklikler kaydedildi.", "Tamam");
        }

        private void CheckForVideoLink()
        {
            if (Description?.Contains("youtube.com/watch?v=") == true)
            {
                HasVideoLink = true;
                var videoId = Description.Split("v=").LastOrDefault();
                if (!string.IsNullOrEmpty(videoId))
                {
                    VideoThumbnail = $"https://img.youtube.com/vi/{videoId}/0.jpg";
                    VideoTitle = "Video";
                }
            }
            else
            {
                HasVideoLink = false;
            }
        }

        private async Task OpenVideo()
        {
            if (HasVideoLink)
            {
                await Browser.OpenAsync(Description, BrowserLaunchMode.SystemPreferred);
            }
        }
    }
} 