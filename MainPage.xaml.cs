using MauiApp1.ViewModels;
using MauiApp1.Models;

namespace MauiApp1
{
    public partial class MainPage : ContentPage
    {
        private MainViewModel ViewModel => BindingContext as MainViewModel;

        public MainPage(MainViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        private void OnCheckBoxCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.BindingContext is TodoItem item)
            {
                ViewModel.ToggleCompletedCommand.Execute(item);
            }
        }
    }
}
