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
    }
}