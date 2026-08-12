---
title: "ZXing.Net.MAUI — Features & Implementation Guidance for Phase 2 (Part 2)"
status: Puplished
part: 2
purpose: "Map ZXing.Net.MAUI (v0.10.3) capabilities to Phase 2 requirements and provide MAUI-specific examples and implementation guidance. Initialization is in how-to-use-zxing.net.maui.md (Part 1)."
audience: "Developers, Implementing Agent"
update_frequency: as-needed
contains:
  - MAUI API mapping
  - Code snippets (CameraBarcodeReaderView, BarcodeReader.DecodeAsync, BarcodeGenerator)
  - Tuning, permissions, lifecycle notes
  - Testing recommendations
---

# ZXing.Net.MAUI — Features & Implementation Guidance for Phase 2

Note: this file targets ZXing.Net.MAUI.Controls v0.10.3 (NuGet). Initialization (.UseBarcodeReader()) is covered in .github/docs/plans/how-to-use-zxing.net.maui.md — this document focuses on MAUI API surface, tuned examples, and Phase 2 mapping.

## Purpose
Map ZXing.Net.MAUI (v0.10.3) capabilities to Phase 2 requirements, provide MAUI-specific examples (CameraBarcodeReaderView, BarcodeReader.DecodeAsync, BarcodeGenerator), and call out Android/platform caveats (permissions, lifecycle, torch/zoom). Use these snippets when implementing Phase 2 epics.

## Key MAUI APIs (v0.10.3)
- CameraBarcodeReaderView (XAML/MAUI view) — live camera scanning control.
- BarcodesDetected event / BarcodeDetectionEventArgs — event raised with detected results.
- BarcodeReader.DecodeAsync(stream, BarcodeReaderOptions) — decode from image streams (MediaPicker flow).
- BarcodeReaderOptions / BarcodeFormats — control which formats to decode and tuning knobs.
- CameraBarcodeReaderView.IsTorchOn — toggle torch.
- CameraBarcodeReaderView.ZoomFactor — normalized 0..1 zoom control.
- CameraBarcodeReaderView.GetAvailableCameras() / SelectedCamera — camera selection.
- BarcodeGeneratorView (XAML) and BarcodeGenerator.WriteToFileAsync / WriteToStreamAsync — generate QR images programmatically.
- BarcodeScanning.IsSupported — check device camera availability.

## Phase 2 feature mapping (MAUI-specific)
- Gallery/static-image scanning: use BarcodeReader.DecodeAsync(stream, BarcodeReaderOptions).
- Live camera scanning and debounce: use CameraBarcodeReaderView + BarcodesDetected event and DelayBetweenContinuousScans options; additionally apply an app-level debounce to avoid duplicate navigation/saves.
- Format filtering: use BarcodeReaderOptions.Formats or BarcodeFormats.TwoDimensional / BarcodeFormats.QrCode to limit CPU usage.
- Torch & zoom: CameraBarcodeReaderView exposes IsTorchOn and ZoomFactor (0..1) — use these first; only implement platform bridges for edge cases.
- Generator: use BarcodeGenerator (WriteToFileAsync/WriteToStreamAsync) or BarcodeGeneratorView for in-UI preview.
- Payload parsing: ZXing returns decoded text only — implement pure C# parsers for WIFI, vCard, VEVENT and test them.

## MAUI-specific examples (concise)
### 1) Camera control in XAML

```xml
<!-- Add xmlns:zxing="clr-namespace:ZXing.Net.Maui.Controls;assembly=ZXing.Net.MAUI.Controls" -->
<zxing:CameraBarcodeReaderView
  x:Name="cameraBarcodeReaderView"
  BarcodesDetected="OnBarcodesDetected"
  IsTorchOn="False"
  ZoomFactor="0"
  HorizontalOptions="FillAndExpand"
  VerticalOptions="FillAndExpand" />
```

### 2) Configure reader options (best placed in code-behind or ViewModel init)

```csharp
cameraBarcodeReaderView.Options = new BarcodeReaderOptions
{
    // Prefer TwoDimensional or explicit QrCode for Phase 2
    Formats = BarcodeFormats.TwoDimensional,
    AutoRotate = true,
    Multiple = false,
    DelayBetweenAnalyzingFrames = 150,           // ms between frames analyzed
    InitialDelayBeforeAnalyzingFrames = 300,     // startup delay
    DelayBetweenContinuousScans = 1000,          // ms between continuous detections
    CameraResolutionSelector = availableResolutions =>
        availableResolutions
            .OrderBy(r => Math.Abs((r.Width * r.Height) - (1280 * 720)))
            .First()
};
```

### 3) Handle detected barcodes with debounce and background persistence

```csharp
private DateTimeOffset _lastScan = DateTimeOffset.MinValue;
private const int APP_DEBOUNCE_MS = 2000; // app-level debounce

protected void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
{
    // e.Results is an IReadOnlyList<BarcodeResult> (Format, Value, etc.)
    var now = DateTimeOffset.UtcNow;
    if ((now - _lastScan).TotalMilliseconds < APP_DEBOUNCE_MS) return;

    var result = e.Results?.FirstOrDefault();
    var text = result?.Value?.Trim();
    if (string.IsNullOrWhiteSpace(text)) return;

    _lastScan = now;

    // Offload classification + DB write to background
    Task.Run(async () => {
        var scan = ClassifyAndCreateScanResult(text, result.Format);
        await _storageService.SaveAsync(scan);
        MainThread.BeginInvokeOnMainThread(() =>
            Shell.Current.GoToAsync($"resultdetail?id={scan.Id}"));
    });
}
```

Notes: the control-level DelayBetweenContinuousScans already helps reduce duplicates; use an app-level debounce for extra safety (history save + navigation atomicity).

### 4) Decode from gallery / static image (MediaPicker)

Note: MediaPicker.PickPhotoAsync() is obsolete in recent MAUI workloads. Use `MediaPicker.PickPhotosAsync()` to allow multi-select and to align with platform guidance; pick the first selected image when only one is needed.

```csharp
// Prefer PickPhotosAsync (returns IEnumerable<FileResult>); select first file
var files = await MediaPicker.Default.PickPhotosAsync();
var file = files?.FirstOrDefault();
if (file == null) return;

await using var stream = await file.OpenReadAsync();
var results = await BarcodeReader.DecodeAsync(
    stream,
    new BarcodeReaderOptions
    {
        // For static-image decoding prefer All to maximize discovery across image sources
        Formats = BarcodeFormats.All,
        AutoRotate = true,
        TryHarder = true,
        Multiple = true
    });

if (results != null)
{
    foreach (var r in results)
    {
        // r.Format, r.Value
    }
}
```

Implementation note: on net10.0 neutral target, stream decoding relies on platform image APIs; prefer performing this on Android/iOS where supported. Resize large images to a reasonable resolution before decoding to conserve CPU.

### 5) Torch toggle & zoom (runtime)

```csharp
// Toggle torch
cameraBarcodeReaderView.IsTorchOn = !cameraBarcodeReaderView.IsTorchOn;

// Set zoom (normalized 0..1)
cameraBarcodeReaderView.ZoomFactor = 0.5f;

// Get cameras
var cams = await cameraBarcodeReaderView.GetAvailableCameras();
cameraBarcodeReaderView.SelectedCamera = cams.FirstOrDefault(c => c.Location == CameraLocation.Rear);
```

The ZoomFactor value is normalized across platforms: 0 = min, 1 = max. Use small steps for pinch-to-zoom mapping and clamp values into [0,1].

### 6) Generator usage (preview + file)

XAML preview

```xml
<zxing:BarcodeGeneratorView
  HeightRequest="200"
  WidthRequest="200"
  Value="https://example.com"
  Format="QrCode"
  ForegroundColor="Black"
  BackgroundColor="White" />
```

Programmatic export

```csharp
var filePath = Path.Combine(FileSystem.AppDataDirectory, "qrcode.png");
await BarcodeGenerator.WriteToFileAsync(
    "https://example.com",
    filePath,
    new BarcodeGeneratorOptions
    {
        Format = BarcodeFormat.QrCode,
        Width = 1024,
        Height = 1024,
        Margin = 2,
        ForegroundColor = Colors.Black,
        BackgroundColor = Colors.White
    });
```

Or write to a stream (for sharing):

```csharp
await using var ms = new MemoryStream();
await BarcodeGenerator.WriteToStreamAsync(
    "https://example.com",
    ms,
    imageOptions: new BarcodeImageOptions { Format = BarcodeImageFormat.Png });
// ms contains PNG bytes
```

## Tuning and performance tips
- Limit formats: use BarcodeFormats.TwoDimensional or BarcodeFormats.QrCode to reduce CPU work.
- Use DelayBetweenAnalyzingFrames and CameraResolutionSelector to balance throughput vs CPU.
- Avoid TryHarder in continuous scanning unless decoding fails frequently — TryHarder increases CPU/time.
- For gallery decoding, downscale very large images before BarcodeReader.DecodeAsync to avoid long pauses.
- Use the control-level DelayBetweenContinuousScans plus an app-level debounce (2000ms) to prevent duplicate saves/navigation.

## Permissions & lifecycle
- Check BarcodeScanning.IsSupported before showing Scanner UI.
- On Android add CAMERA permission to AndroidManifest and request runtime permission before enabling scanning.
- Prefer MediaPicker for gallery access (avoids broad READ_EXTERNAL_STORAGE on recent Android versions); if direct file access is needed, request READ_MEDIA_IMAGES where required.
- Stop scanning and unsubscribe event handlers in OnDisappearing to free camera resources:

```csharp
protected override void OnDisappearing()
{
    base.OnDisappearing();
    cameraBarcodeReaderView.IsAnalyzing = false; // or IsScanning = false depending on API
    cameraBarcodeReaderView.BarcodesDetected -= OnBarcodesDetected;
}
```

## Payload parsing guidance
- ZXing returns decoded text and format only. Implement separate, pure C# parsers for WiFi (WIFI:T:...), vCard (BEGIN:VCARD), and VEVENT. Keep these parsers free of platform calls for unit testing.
- For contact/calendar actions prefer launching platform intents (Add contact / Add calendar event) rather than direct writes to device stores, to minimize required permissions.

## Testing recommendations
- Add image test vectors (valid QR, noisy QR, truncated) as test assets and assert BarcodeReader.DecodeAsync finds expected text.
- Verify generated QR PNGs by decoding them in unit tests with BarcodeReader to ensure scannability.
- Manually test torch and zoom on devices with and without hardware capabilities; emulator behavior may differ.

## CI & reproducibility
- Pin ZXing.Net.MAUI.Controls package to 0.10.3 in csproj to avoid API drift.
- Unit tests can run without MAUI UI workloads; include parser and generator round-trip tests that use BarcodeGenerator.WriteToStreamAsync and BarcodeReader.DecodeAsync on supported platforms or via platform-specific test runners.

## References
- ZXing.Net.MAUI.Controls NuGet (v0.10.3) — API notes and samples
- .github/docs/plans/how-to-use-zxing.net.maui.md — project-local initialization and setup (do not duplicate initialization here)

## Conclusion
This document replaces generic ZXing examples with MAUI-specific APIs present in ZXing.Net.MAUI.Controls v0.10.3. The prior guidance on pure-parsers, background work, debouncing, and DB persistence remains valid. Use CameraBarcodeReaderView, BarcodeReader.DecodeAsync, BarcodeGenerator and the listed tuning knobs for Phase 2 implementation.