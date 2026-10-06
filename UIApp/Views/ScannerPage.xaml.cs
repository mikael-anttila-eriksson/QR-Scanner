using System.Diagnostics;
using UIApp.ViewModels;
using ZXing.Net.Maui.Controls;
using ZXing.Net.Maui;

namespace UIApp.Views
{
    public partial class ScannerPage : ContentPage
    {
        private readonly ScannerViewModel _viewModel;

        public ScannerPage(ViewModels.ScannerViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = viewModel;
            ConfigureBarcodeReader();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.RequestCameraPermissionAsync();
            // Pass camera view reference to ViewModel for torch control
            _viewModel.SetCameraView(CameraBarcodeReaderView);

            // Sync current zoom
            try
            {
                _currentZoomFactor = CameraBarcodeReaderView?.ZoomFactor ?? 0f;
            }
            catch (Exception ex) { Debug.WriteLine($"OnAppearing exception: {ex}"); }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            // Stop scanning and clean up to free camera resources
            CameraBarcodeReaderView.IsEnabled = false;
            _viewModel.SetCameraView(null);
        }

        private void ConfigureBarcodeReader()
        {
            // QR codes only (MVP scope)
            CameraBarcodeReaderView.Options = new BarcodeReaderOptions
            {
                Formats = BarcodeFormat.QrCode,
                AutoRotate = true,
                TryInverted = false
            };
        }

        private async void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs args)
        {
            if (args?.Results == null || args.Results.Any() == false)
                return;

            var barcode = args.Results.FirstOrDefault();
            if (barcode == null) return;

            var rawValue = barcode.Value;

            // Process through ViewModel (debounce is handled in ScannerService)
            await _viewModel.ProcessScannedTextAsync(rawValue);
        }

        /// <summary>
        /// Tracks the active zoom level across the lifetime of the application. Between 0 and 1.
        /// </summary>
        private float _currentZoomFactor = 0f;
        /// <summary>
        /// Maximum zoom factor (1.0 = 100% zoom).
        /// </summary>
        private const float _maxZoomF = 1f;
        /// <summary>
        /// Minimum zoom factor (0.0 = no zoom).
        /// </summary>
        private const float _minZoomF = 0f;
        /// <summary>
        /// Adjust this to change how fast or slow the camera zooms.
        /// </summary>
        private const float _sensitivity = 0.5f;
        /// <summary>
        /// Handles pinch gesture updates to adjust the camera's zoom factor. The zoom factor is clamped between _minZoomF and _maxZoomF, and the current zoom level is tracked in _currentZoomFactor. The HUD is updated to show the current zoom level, and it is hidden after a short delay when the pinch gesture is completed or canceled.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
        {
            try
            {
                if (CameraBarcodeReaderView == null) return;

                if (e.Status == GestureStatus.Started)
                {

                }
                else if (e.Status == GestureStatus.Running)
                {
                    // e.Scale > 1 = pinching out (zoom in), < 1 = pinching in (zoom out)

                    var delta = (float)(e.Scale - 1.0) * _sensitivity;
                    _currentZoomFactor += delta;
                    _currentZoomFactor = Math.Clamp(_currentZoomFactor, _minZoomF, _maxZoomF);

                    // Apply zoom factor on the main thread
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        try { CameraBarcodeReaderView.ZoomFactor = _currentZoomFactor; }
                        catch (Exception ex) { Debug.WriteLine($"Failed to set ZoomFactor during pinch: {ex}"); }

                        Debug.WriteLine($"Pinch Running: scale={(double)e.Scale:0.00}, applied={_currentZoomFactor:0.000}");

                        // Update HUD and viewmodel status with normalized 0..1 value as requested
                        var normalizedDisplay = Math.Round(_currentZoomFactor, 2);
                        _viewModel.StatusMessage = $"Zoom: {normalizedDisplay:0.##}";

                        try { ZoomHudLabel.Text = $"Zoom: {normalizedDisplay:0.##}"; ZoomHud.IsVisible = true; } catch { }
                    });
                }
                else if (e.Status == GestureStatus.Completed || e.Status == GestureStatus.Canceled)
                {
                    // Hide HUD after a short delay
                    MainThread.BeginInvokeOnMainThread(async () => { await Task.Delay(900); try { ZoomHud.IsVisible = false; } catch { } });
                    _viewModel.StatusMessage = "Align QR code in frame";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Pinch handler exception: {ex}");
            }
        }
    }
}
