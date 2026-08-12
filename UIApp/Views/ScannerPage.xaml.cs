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

        // Pinch-to-zoom handler: maps pinch scale to CameraBarcodeReaderView.ZoomFactor (0..1)
        private float _startZoom = 0f;
        private void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
        {
            try
            {
                if (CameraBarcodeReaderView == null)
                    return;

                if (e.Status == GestureStatus.Started)
                {
                    _startZoom = CameraBarcodeReaderView.ZoomFactor;
                }
                else if (e.Status == GestureStatus.Running)
                {
                    // e.Scale is the relative scale since gesture start
                    var newZoom = (double)(_startZoom * (float)e.Scale);
                    // Clamp to [0,1]
                    newZoom = Math.Max(0.0, Math.Min(1.0, newZoom));
                    CameraBarcodeReaderView.ZoomFactor = (float)newZoom;
                    // Update status message with percent (user feedback)
                    _viewModel.StatusMessage = $"Zoom: {Math.Round(newZoom * 100)}%";
                }
                else if (e.Status == GestureStatus.Completed || e.Status == GestureStatus.Canceled)
                {
                    // restore default status after gesture ends
                    _viewModel.StatusMessage = "Align QR code in frame";
                }
            }
            catch
            {
                // ignore pinch errors
            }
        }
    }
}