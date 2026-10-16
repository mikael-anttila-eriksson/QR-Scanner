using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UIApp.Models;
using UIApp.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace UIApp.ViewModels
{
    public partial class HistoryViewModel : BaseViewModel
    {
        private readonly SqliteStorageService _storage;
        private const string PREFS_FAVORITES_ONLY = "prefs.favorites_only";

        public ObservableCollection<ScanResult> Items { get; } = new ObservableCollection<ScanResult>();

        [ObservableProperty]
        public partial bool IsEmpty { get; set; } = true;

        [ObservableProperty]
        public partial bool ShowFavoritesOnly { get; set; } = false;

        public HistoryViewModel(SqliteStorageService storage)
        {
            _storage = storage;
            // Load persisted preference
            ShowFavoritesOnly = Preferences.Get(PREFS_FAVORITES_ONLY, false);
        }

        // Called when the generated ShowFavoritesOnly property changes
        partial void OnShowFavoritesOnlyChanged(bool value)
        {
            try
            {
                Preferences.Set(PREFS_FAVORITES_ONLY, value);
                // Fire and forget reload (UI will update when complete)
                _ = LoadHistory();
            }
            catch
            {
                // ignore preference save errors
            }
        }

        [RelayCommand]
        public async Task LoadHistory()
        {
            try
            {
                await _storage.InitializeAsync();
                System.Collections.Generic.List<ScanResult> list;
                if (ShowFavoritesOnly)
                {
                    list = await _storage.GetFavoritesAsync();
                }
                else
                {
                    list = await _storage.GetAllAsync();
                }

                Items.Clear();
                foreach (var r in list)
                    Items.Add(r);

                IsEmpty = Items.Count == 0;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", $"Failed to load history: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        public async Task ItemSelected(ScanResult item)
        {
            if (item == null) return;

            try
            {
                await Shell.Current.GoToAsync($"{AppShell.ResultDetailRoute}?id={item.Id}");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", $"Navigation failed: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        public async Task Delete(ScanResult item)
        {
            if (item == null) return;

            try
            {
                await _storage.DeleteAsync(item);
                Items.Remove(item);
                IsEmpty = Items.Count == 0;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", $"Could not delete: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        public async Task ClearAll()
        {
            try
            {
                var confirm = await Shell.Current.DisplayAlertAsync("Clear history", "Delete all scan history? This cannot be undone.", "Delete", "Cancel");
                if (!confirm) return;

                await _storage.ClearAllAsync();
                Items.Clear();
                IsEmpty = true;

                await Shell.Current.DisplayAlertAsync("History", "All scan history deleted.", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", $"Failed to clear history: {ex.Message}", "OK");
            }
        }
    }
}