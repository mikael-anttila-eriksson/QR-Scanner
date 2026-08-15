# How to Use ZXing.Net.MAUI for QR Code Scanning

## Overview

This document provides a complete guide for integrating **ZXing.Net.MAUI** (also known as `ZXing.Net.Maui.Controls`) into the QR Scanner application. It covers package installation, setup, and implementation patterns specific to this project''s MVP requirements.

**Project Context:**
- Target: Android only (.NET 10)
- Framework: .NET MAUI
- Use case: Live QR code scanning from camera
- Navigation: Shell-based (TabBar with Scanner and History tabs)
- Storage: SQLite persistence

---

## 1. Package Installation

### 1.1 Required NuGet Packages

Add the following packages to `UIApp.csproj`:

```xml
<ItemGroup>
    <!-- Primary barcode/QR scanning library for MAUI -->
    <PackageReference Include="ZXing.Net.Maui" Version="0.4.0" />
    
    <!-- SQLite persistence (as per spec) -->
    <PackageReference Include="sqlite-net-pcl" Version="1.8.116" />
    <PackageReference Include="SQLitePCLRaw.bundle_green" Version="2.1.5" />
    
    <!-- MVVM source generators -->
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
    
    <!-- MAUI Essentials for permissions, clipboard, share, launcher -->
    <PackageReference Include="Microsoft.Maui.Controls.Hosting" Version="10.0.51" />
    <PackageReference Include="Microsoft.Maui.Essentials" Version="10.0.51" />
</ItemGroup>
```

**Version Notes:**
- Check latest stable versions at [NuGet.org](https://www.nuget.org/)
- Pin exact versions to avoid unexpected breaking changes
- Ensure ZXing.Net.Maui version is compatible with .NET 10 and MAUI 10.x

### 1.2 Installation Command

```bash
dotnet add UIApp package ZXing.Net.Maui.Controls --version 0.10.3
dotnet add UIApp package sqlite-net-pcl --version 1.8.116
dotnet add UIApp package SQLitePCLRaw.bundle_green --version 2.1.5
dotnet add UIApp package CommunityToolkit.Mvvm --version 8.2.2
```

---

## 2. Initial Setup

### 2.1 Android Permissions

ZXing.Net.MAUI requires camera access. Add the `CAMERA` permission to `Platforms/Android/AndroidManifest.xml`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<manifest xmlns:android="http://schemas.android.com/apk/res/android">
    <uses-permission android:name="android.permission.CAMERA" />
    <uses-feature android:name="android.hardware.camera" android:required="false" />
    
    <application>
        <!-- Your application content -->
    </application>
</manifest>
```

**Important:**
- `android:required="false"` allows the app to run on devices without a camera (graceful degradation).
- Runtime permission handling is implemented in the Scanner page (see section 3).

### 2.2 MauiProgram.cs Initialization

Register ZXing.Net.MAUI and configure SQLite in `MauiProgram.cs`:

```csharp
using ZXing.Net.Maui.Controls;
using SQLitePCL;

namespace QrScannerApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        // Initialize native SQLite provider (required by sqlite-net-pcl)
        Batteries_V2.Init();
        
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            })
            // Add ZXing controls to MAUI
            .UseBarcodeReader()
            
            // Set dark theme globally
            .ConfigureAppTheme();

        // Register services
        builder.Services
            .AddSingleton<ScannerPage>()
            .AddSingleton<ScannerViewModel>()
            .AddSingleton<HistoryPage>()
            .AddSingleton<HistoryViewModel>()
            .AddSingleton<ResultDetailPage>()
            .AddSingleton<ResultDetailViewModel>()
            .AddSingleton<SqliteStorageService>()
            .AddSingleton<ScannerService>();

        return builder.Build();
    }

    private static MauiAppBuilder ConfigureAppTheme(this MauiAppBuilder builder)
    {
        // Enforce dark theme regardless of system settings
        Application.Current!.UserAppTheme = AppTheme.Dark;
        return builder;
    }
}
```

**Key Points:**
- `Batteries_V2.Init()` must be called before any database access
- `.UseBarcodeReader()` registers the ZXing MAUI controls
- `Application.Current.UserAppTheme` sets dark theme globally

---

## 3. Scanner Page Implementation

### 3.1 XAML Layout (ScannerPage.xaml)

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage 
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:zxing="clr-namespace:ZXing.Net.Maui.Controls;assembly=ZXing.Net.Maui.Controls"
    x:Class="QrScannerApp.Views.ScannerPage"
    x:DataType="local:ScannerViewModel"
    Title="Scan"
    BackgroundColor="#1e1e1e">

    <Grid RowDefinitions="*,Auto" Padding="0">
        <!-- Camera View with Overlay -->
        <Grid RowDefinitions="*" ColumnDefinitions="*">
            <!-- Full-screen camera preview -->
            <zxing:CameraBarcodeReaderView 
                x:Name="CameraBarcodeReaderView"
                BarcodesDetected="OnBarcodesDetected"
                IsEnabled="{Binding IsScanningEnabled}"
                Grid.Row="0"
                Grid.Column="0" />

            <!-- Semi-transparent overlay outside viewfinder -->
            <BoxView 
                Grid.Row="0" 
                Grid.Column="0"
                BackgroundColor="#00000080"
                IsVisible="{Binding IsOverlayVisible}" />

            <!-- Viewfinder rectangle (border only) -->
            <Border 
                Grid.Row="0"
                Grid.Column="0"
                Margin="40"
                Stroke="#00ff00"
                StrokeThickness="3"
                HasShadow="False">
                <Border.StrokeShape>
                    <RoundRectangle CornerRadius="12" />
                </Border.StrokeShape>
            </Border>
        </Grid>

        <!-- Status text at bottom -->
        <Label 
            Grid.Row="1"
            Text="{Binding StatusMessage}"
            TextColor="#ffffff"
            FontSize="14"
            HorizontalTextAlignment="Center"
            Padding="20,16"
            BackgroundColor="#2a2a2a" />
    </Grid>
</ContentPage>
```

**Layout Notes:**
- `CameraBarcodeReaderView` fills the screen with live camera feed
- Overlay `BoxView` with `#00000080` (semi-transparent black) dims outside the viewfinder
- `Border` acts as the viewfinder rectangle (green stroke)
- `StatusMessage` label shows scan feedback at the bottom

---

## 4. Data Model & Storage

### 4.1 ScanResult Model

```csharp
using SQLite;

namespace QrScannerApp.Models;

public enum ScanResultType
{
    Url,
    PlainText
}

public class ScanResult
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string RawValue { get; set; } = string.Empty;

    public ScanResultType Type { get; set; }

    public DateTime ScannedAt { get; set; }

    public bool IsFavorite { get; set; } = false; // For Phase 2
}
```

### 4.2 SqliteStorageService

```csharp
using SQLite;
using QrScannerApp.Models;

namespace QrScannerApp.Services;

public class SqliteStorageService
{
    private readonly SQLiteAsyncConnection _database;
    private const string DbName = "qrscanner.db3";

    public SqliteStorageService()
    {
        var databasePath = Path.Combine(
            FileSystem.AppDataDirectory,
            DbName);

        _database = new SQLiteAsyncConnection(databasePath);
    }

    public async Task InitializeAsync()
    {
        await _database.CreateTableAsync<ScanResult>();
    }

    public async Task<ScanResult> CreateScanResultAsync(ScanResult scanResult)
    {
        await _database.InsertAsync(scanResult);
        return scanResult;
    }

    public async Task<ScanResult?> GetScanResultByIdAsync(int id)
    {
        return await _database.Table<ScanResult>()
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<List<ScanResult>> GetAllScanResultsAsync()
    {
        return await _database.Table<ScanResult>()
            .OrderByDescending(s => s.ScannedAt)
            .ToListAsync();
    }

    public async Task DeleteScanResultAsync(int id)
    {
        await _database.DeleteAsync<ScanResult>(id);
    }
}
```

---

## 5. CameraBarcodeReaderView Configuration

### 5.1 Event Handler & Debounce

```csharp
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace QrScannerApp.Views;

public partial class ScannerPage : ContentPage
{
    private readonly ScannerViewModel _viewModel;
    private const double SCAN_DEBOUNCE_MS = 2000;
    private DateTime _lastScanTime = DateTime.MinValue;

    public ScannerPage(ScannerViewModel viewModel)
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

        // 2-second debounce to prevent duplicate scans
        var timeSinceLastScan = DateTime.Now - _lastScanTime;
        if (timeSinceLastScan.TotalMilliseconds < SCAN_DEBOUNCE_MS)
            return;

        _lastScanTime = DateTime.Now;

        var barcode = args.Results.FirstOrDefault();
        var rawValue = barcode.Value;

        // Validate: reject empty/whitespace
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            await DisplayAlert("", "Couldn''t read QR code, try again.", "OK");
            return;
        }

        // Classify and save
        await _viewModel.ProcessScanResultAsync(rawValue);
    }
}
```

### 5.2 BarcodeDetectionEventArgs Properties

The `BarcodeDetectionEventArgs` contains:
- `Results` (List<BarcodeResult>): All detected barcodes in the frame
- Each `BarcodeResult` has:
  - `Value` (string): Decoded content (the QR payload)
  - `Format` (BarcodeFormat): Barcode type (QrCode, EAN13, etc.)
  - `BoundingBox` (Rect): Position on camera frame (optional for MVP)

---

## 6. Result Classification & Actions

### 6.1 URL Detection

```csharp
public class ScannerService
{
    public ScanResultType ClassifyResult(string rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
            return ScanResultType.PlainText;

        // Only http/https are classified as URL in MVP
        if (Uri.TryCreate(rawValue, UriKind.Absolute, out var uri))
        {
            if (uri.Scheme is "http" or "https")
                return ScanResultType.Url;
        }

        return ScanResultType.PlainText;
    }
}
```

### 6.2 Action Implementation

```csharp
// Open in Browser (URL only)
await Launcher.OpenAsync(new Uri(scanResult.RawValue));

// Copy to Clipboard
await Clipboard.SetTextAsync(scanResult.RawValue);

// Share via Android Share Sheet
await Share.RequestAsync(new ShareTextRequest
{
    Text = scanResult.RawValue,
    Title = "Share QR Code Content"
});
```

---

## 7. Permission Handling

### 7.1 Camera Permission Request

```csharp
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
                    await AppInfo.ShowSettingsAsync();
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
```

---

## 8. Common Issues & Troubleshooting

| Issue | Solution |
|-------|----------|
| **Black screen / No camera feed** | Verify ZXing.Net.Maui version compatibility with .NET 10; check Android manifest CAMERA permission; test on physical device. Also ensure the MAUI builder extension matches the installed ZXing package — call `builder.UseBarcodeReader()` (and include `using ZXing.Net.Maui.Controls;`) instead of `UseBarcodeScanningView()` which is from a different library variant and causes a missing-extension error. |
| **Multiple scans from one code** | Increase SCAN_DEBOUNCE_MS (default 2000ms); verify `_lastScanTime` is updated before processing |
| **SQLite errors** | Call `SQLitePCL.Batteries_V2.Init()` before DB access; verify `InitializeAsync()` is called at app startup |
| **Permission not requested** | Call `RequestCameraPermissionAsync()` in `ScannerPage.OnAppearing()`; verify manifest has CAMERA permission |
| **Query parameter not binding** | Ensure `[QueryProperty(nameof(Id), "id")]` on ViewModel; check `OnIdChanged` partial method is defined |

---

## 9. Performance Best Practices

- **Debounce tuning:** 2s balances responsiveness vs. duplicate prevention
- **Resource cleanup:** Disable scanning when navigating to reduce CPU/battery usage
- **SQLite:** Use `SQLiteAsyncConnection` for async-safe operations
- **Camera:** Test on real Android devices (emulator camera support varies)

---

## 10. Next Steps (Phase 2)

- Gallery image scanning via `MediaPicker`/`FilePicker`
- Torch/flashlight toggle
- Pinch-to-zoom
- WiFi/vCard/calendar payload parsing
- Favorites filter in History
- Settings (sound toggle, haptic feedback)

---

## References

- **Project Spec:** `.github/docs/plans/qr-scanner-app-spec-mvp-phase2.md`
- **Implementation Plan:** `.github/docs/plans/qr-scanner-mvp--implementation-plan.md`
- **Architecture:** `.github/docs/architecture.md`
- **ZXing.Net.MAUI:** https://github.com/micjahn/zxing.net
- **.NET MAUI Shell:** https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/shell/
- **MAUI Permissions:** https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/appmodel/permissions
