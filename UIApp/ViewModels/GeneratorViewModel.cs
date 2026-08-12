using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Storage;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Controls;
using System;
using ZXing.Net.Maui;

namespace UIApp.ViewModels
{
    public partial class GeneratorViewModel : BaseViewModel
    {
        private const int MAX_LENGTH = 512;

        [ObservableProperty]
        private string content = string.Empty;

        [ObservableProperty]
        private ImageSource generatedImageSource;

        [ObservableProperty]
        private bool isImageAvailable;

        public string CharCountDisplay => $"{(Content?.Length ?? 0)}/{MAX_LENGTH}";

        public bool CanGenerate => !string.IsNullOrWhiteSpace(Content) && (Content.Length <= MAX_LENGTH);

        public GeneratorViewModel()
        {
            Content = string.Empty;
            GeneratedImageSource = null;
            IsImageAvailable = false;
        }

        partial void OnContentChanged(string value)
        {
            OnPropertyChanged(nameof(CharCountDisplay));
            OnPropertyChanged(nameof(CanGenerate));
        }

        [RelayCommand]
        public async Task Generate()
        {
            try
            {
                if (!CanGenerate) return;

                // Use ZXing BarcodeGenerator to write PNG into memory
                using var ms = new MemoryStream();

                var options = new BarcodeGeneratorOptions
                {
                    Format = BarcodeFormat.QrCode,
                    Width = 1024,
                    Height = 1024,
                    // High contrast black/white defaults
                };

                // Write PNG to stream
                await BarcodeGenerator.WriteToStreamAsync(options, ms, imageOptions: new BarcodeImageOptions { Format = BarcodeImageFormat.Png });

                ms.Seek(0, SeekOrigin.Begin);
                var bytes = ms.ToArray();

                GeneratedImageSource = ImageSource.FromStream(() => new MemoryStream(bytes));
                IsImageAvailable = true;
                OnPropertyChanged(nameof(GeneratedImageSource));
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to generate QR: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        public async Task Save()
        {
            try
            {
                if (!IsImageAvailable)
                {
                    await Shell.Current.DisplayAlert("Save", "No generated image to save.", "OK");
                    return;
                }

                // Create image again into memory stream and save file
                using var ms = new MemoryStream();
                var options = new BarcodeGeneratorOptions { Format = BarcodeFormat.QrCode, Width = 1024, Height = 1024 };
                await BarcodeGenerator.WriteToStreamAsync(options, ms, imageOptions: new BarcodeImageOptions { Format = BarcodeImageFormat.Png });
                ms.Seek(0, SeekOrigin.Begin);

                var filename = $"qr_{DateTime.UtcNow:yyyyMMddHHmmss}.png";
                var path = Path.Combine(FileSystem.AppDataDirectory, filename);
                File.WriteAllBytes(path, ms.ToArray());

                await Shell.Current.DisplayAlert("Saved", $"Saved to: {path}", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to save image: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        public async Task Share()
        {
            try
            {
                if (!IsImageAvailable)
                {
                    await Shell.Current.DisplayAlert("Share", "No generated image to share.", "OK");
                    return;
                }

                // Regenerate into memory to share
                using var ms = new MemoryStream();
                var options = new BarcodeGeneratorOptions { Format = BarcodeFormat.QrCode, Width = 1024, Height = 1024 };
                await BarcodeGenerator.WriteToStreamAsync(options, ms, imageOptions: new BarcodeImageOptions { Format = BarcodeImageFormat.Png });
                ms.Seek(0, SeekOrigin.Begin);

                var tempName = Path.Combine(FileSystem.CacheDirectory, $"qr_share_{DateTime.UtcNow:yyyyMMddHHmmss}.png");
                File.WriteAllBytes(tempName, ms.ToArray());

                var request = new ShareFileRequest
                {
                    Title = "Share QR",
                    File = new ShareFile(tempName)
                };

                await Share.RequestAsync(request);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to share image: {ex.Message}", "OK");
            }
        }
    }
}