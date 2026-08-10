using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UIApp.Models;
using UIApp.Services;
using Microsoft.Maui.Controls;

namespace UIApp.ViewModels
{
    public partial class HistoryViewModel : BaseViewModel
    {
        private readonly SqliteStorageService _storage;

        public ObservableCollection<ScanResult> Items { get; } = new ObservableCollection<ScanResult>();

        [ObservableProperty]
        private bool isEmpty = true;

        public HistoryViewModel(SqliteStorageService storage)
        {
            _storage = storage;
        }

        [RelayCommand]
        public async Task LoadHistory()
        {
            try
            {
                await _storage.InitializeAsync();
                var list = await _storage.GetAllAsync();
                Items.Clear();
                foreach (var r in list)
                    Items.Add(r);

                IsEmpty = Items.Count == 0;
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert("Error", $"Failed to load history: {ex.Message}", "OK");
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
                await Application.Current!.MainPage!.DisplayAlert("Error", $"Navigation failed: {ex.Message}", "OK");
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
                await Application.Current!.MainPage!.DisplayAlert("Error", $"Could not delete: {ex.Message}", "OK");
            }
        }
    }
}