using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UIApp.Models;
using UIApp.Services;
using Microsoft.Maui.Essentials;

namespace UIApp.ViewModels
{
    public partial class ResultDetailViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly SqliteStorageService _storage;

        [ObservableProperty]
        public partial ScanResult? CurrentResult { get; set; }

        public ResultDetailViewModel(SqliteStorageService storage)
        {
            _storage = storage;
        }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("id") && int.TryParse(query["id"].ToString(), out var id))
            {
                await LoadResultAsync(id);
            }
        }

        private async Task LoadResultAsync(int id)
        {
            try
            {
                await _storage.InitializeAsync();
                CurrentResult = await _storage.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", $"Failed to load result: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        public async Task OpenUrl()
        {
            if (CurrentResult == null || CurrentResult.Type != "URL")
                return;

            try
            {
                await Launcher.OpenAsync(new Uri(CurrentResult.RawValue));
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", $"Could not open URL: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        public async Task CopyToClipboard()
        {
            if (CurrentResult == null)
                return;

            try
            {
                await Clipboard.SetTextAsync(CurrentResult.RawValue);
                await Shell.Current.DisplayAlertAsync("", "Copied to clipboard", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", $"Could not copy: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        public async Task ShareResult()
        {
            if (CurrentResult == null)
                return;

            try
            {
                await Share.RequestAsync(new ShareTextRequest
                {
                    Text = CurrentResult.RawValue,
                    Title = $"Share QR Code ({CurrentResult.Type})"
                });
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", $"Could not share: {ex.Message}", "OK");
            }
        }
    }
}