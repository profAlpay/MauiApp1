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
        private TodoItem _task;
        private string _videoThumbnail;
        private string _videoTitle;
        private bool _hasVideoLink;

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

        public string Title { get; private set; }
        public string Description { get; private set; }
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

        public TaskDetailViewModel()
        {
            OpenVideoCommand = new Command(async () => await OpenVideo());
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
                    // Video başlığını API ile çekebilirsiniz (YouTube Data API gerekir)
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