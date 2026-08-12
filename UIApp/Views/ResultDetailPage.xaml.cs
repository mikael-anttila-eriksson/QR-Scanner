using UIApp.ViewModels;
using Microsoft.Maui.ApplicationModel;

namespace UIApp.Views
{
    public partial class ResultDetailPage : ContentPage
    {
        private readonly ResultDetailViewModel _viewModel;

        public ResultDetailPage(ViewModels.ResultDetailViewModel vm)
        {
            InitializeComponent();
            _viewModel = vm;
            BindingContext = vm;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            // Enable Open button only if result type is URL
            if (_viewModel.CurrentResult != null)
            {
                OpenButton.IsEnabled = _viewModel.CurrentResult.Type == "URL";
            }

            // Update favorite button text based on current state
            UpdateFavoriteButtonText();

            // Subscribe to property changes to update favorite button when toggled
            _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_viewModel.CurrentResult))
            {
                MainThread.BeginInvokeOnMainThread(UpdateFavoriteButtonText);
            }
        }

        private void UpdateFavoriteButtonText()
        {
            if (FavoriteButton == null || _viewModel.CurrentResult == null) return;
            FavoriteButton.Text = _viewModel.CurrentResult.IsFavorite ? "★ Favorite" : "☆ Favorite";
        }
    }
}