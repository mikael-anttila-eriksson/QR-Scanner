using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using UIApp.Models;
using UIApp.Services;

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

                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.Camera>();

                    if (status != PermissionStatus.Granted)
                    {
                        var result = await Application.Current!.MainPage!.DisplayAlert(
                            "Camera Permission Required",
                            "The camera permission is required to scan QR codes. Please enable it in app settings.",
                            "Open Settings",
                            "Cancel");

                        if (result)
                        {
                            // Navigate to app settings on Android
#if ANDROID
                            await Launcher.OpenAsync(new Uri("android://intent/#Intent;action=android.settings.APP_NOTIFICATION_SETTINGS;package=" + AppInfo.PackageName + ";end"));
#endif
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert(
                    "Permission Error",
                    $"Failed to request camera permission: {ex.Message}",
                    "OK");
            }
        }

        public async Task ProcessScannedTextAsync(string rawValue)
        {
            try
            {
                var scanResult = await _scannerService.ProcessScannedBarcodeAsync(rawValue);

                if (scanResult == null)
                {
                    await Application.Current!.MainPage!.DisplayAlert("", "Couldn't read QR code, try again.", "OK");
                    return;
                }

                StatusMessage = $"Scanned: {(scanResult.Type == "URL" ? "Link" : "Text")}";

                // Navigate to detail page
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Shell.Current.GoToAsync($"resultdetail?id={scanResult.Id}");
                });
            }
            catch (Exception ex)
            {
                StatusMessage = "Error scanning";
                await Application.Current!.MainPage!.DisplayAlert("Error", ex.Message, "OK");
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
