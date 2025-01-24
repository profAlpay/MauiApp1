using Microsoft.Maui.Controls;
using MauiApp1.ViewModels;

namespace MauiApp1.Views
{
    public partial class TaskDetailPage : ContentPage
    {
        private TaskDetailViewModel ViewModel => BindingContext as TaskDetailViewModel;

        public TaskDetailPage()
        {
            InitializeComponent();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            // Sayfa kapanırken düzenleme modunu sıfırla
            if (ViewModel != null)
            {
                ViewModel.IsEditing = false;
            }
        }
    }
} 