using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UIApp.Models;
using UIApp.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Essentials;
using Microsoft.Maui.Controls;

namespace UIApp.ViewModels
{
    public partial class ScannerViewModel : BaseViewModel
    {
        private readonly ScannerService _scannerService;

        [ObservableProperty]
        public partial bool IsScanningEnabled { get; set; }

        [ObservableProperty]
        public partial bool IsOverlayVisible { get; set; }

        [ObservableProperty]
        public partial string StatusMessage { get; set; } = "Align QR code in frame";

        public ScannerViewModel(ScannerService scannerService)
        {
            _scannerService = scannerService;
        }

        public async Task RequestCameraPermissionAsync()
        {
            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.Camera>();

                if (status == PermissionStatus.Granted)
                {
                    IsScanningEnabled = true;
                    IsOverlayVisible = false;
                    StatusMessage = "Align QR code in frame";
                    return;
                }

                status = await Permissions.RequestAsync<Permissions.Camera>();

                if (status == PermissionStatus.Granted)
                {
                    IsScanningEnabled = true;
                    IsOverlayVisible = false;
                    StatusMessage = "Align QR code in frame";
                    return;
                }

                // Permission denied — show overlay and guidance
                IsScanningEnabled = false;
                IsOverlayVisible = true;
                StatusMessage = "Camera permission denied. Open settings to enable camera.";

                var open = await Shell.Current.DisplayAlertAsync(
                    "Camera Permission Required",
                    "The camera permission is required to scan QR codes.",
                    "Open Settings",
                    "Cancel");

                if (open)
                {
                    try
                    {
                        AppInfo.ShowSettingsUI();
                    }
                    catch
                    {
                        // Last resort: try Launcher with generic settings URI
                        try { await Launcher.OpenAsync(new Uri("app-settings:")); } catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync(
                    "Permission Error",
                    $"Failed to request camera permission: {ex.Message}",
                    "OK");
            }
        }

        [RelayCommand]
        public void OpenAppSettings()
        {
            try
            {
                AppInfo.ShowSettingsUI();
            }
            catch
            {
                // ignore
            }
        }

        [RelayCommand]
        public async Task PickGalleryImage()
        {
            try
            {
                StatusMessage = "Opening gallery...";
                
                // Open media picker to select an image from gallery
                var result = await MediaPicker.PickPhotoAsync();
                if (result == null)
                {
                    // User cancelled
                    StatusMessage = "Align QR code in frame";
                    return;
                }

                StatusMessage = "Decoding image...";

                // Decode the image using ScannerService
                var scanResult = await _scannerService.DecodeImageAsync(result);

                if (scanResult == null)
                {
                    StatusMessage = "No QR code found in image";
                    await Shell.Current.DisplayAlertAsync(
                        "Decode Failed",
                        "No QR code was found in the selected image. Please try another image.",
                        "OK");
                    StatusMessage = "Align QR code in frame";
                    return;
                }

                StatusMessage = $"Found: {(scanResult.Type == "URL" ? "Link" : "Text")}";

                // Navigate to detail page
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Shell.Current.GoToAsync($"{AppShell.ResultDetailRoute}?id={scanResult.Id}");
                });
            }
            catch (Exception ex)
            {
                StatusMessage = "Error decoding image";
                await Shell.Current.DisplayAlertAsync(
                    "Error",
                    $"Failed to decode image: {ex.Message}",
                    "OK");
                StatusMessage = "Align QR code in frame";
            }
        }

        public async Task ProcessScannedTextAsync(string rawValue)
        {
            try
            {
                var scanResult = await _scannerService.ProcessScannedBarcodeAsync(rawValue);

                if (scanResult == null)
                {
                    await Shell.Current.DisplayAlertAsync("", "Couldn't read QR code, try again.", "OK");
                    return;
                }

                StatusMessage = $"Scanned: {(scanResult.Type == "URL" ? "Link" : "Text")}";

                // Navigate to detail page
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Shell.Current.GoToAsync($"{AppShell.ResultDetailRoute}?id={scanResult.Id}");
                });
            }
            catch (Exception ex)
            {
                StatusMessage = "Error scanning";
                await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }

        private bool IsLikelyUrl(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;
            return Uri.IsWellFormedUriString(text, UriKind.Absolute) || 
                   Regex.IsMatch(text, @"^https?://", RegexOptions.IgnoreCase);
        }
    }
}
