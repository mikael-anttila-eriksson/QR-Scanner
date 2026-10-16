# How To: Pinch-to-Zoom with Magnification Display

**Status:** Phase 2 (torch/zoom)
**Scope:** ScannerPage only — code-behind (`.xaml.cs`), no ViewModel
**Platform:** Android only (API 34 target); no-op on Windows via existing `MockScannerService`

## What this implements

Two-finger pinch on the camera preview adjusts zoom. Current magnification is shown
as a multiplier label ("1.0x", "2.3x") — not a percentage. "x" matches the zoom
ratio ZXing.Net.MAUI/CameraX already work in natively, so no conversion is needed
between what's displayed and what's sent to the camera.

## Why code-behind, not MVVM

This app follows MVVM (`BaseViewModel` + CommunityToolkit.Mvvm) for everything else.
Pinch-gesture state is the exception: `_startZoomRatio` and the running scale
calculation are page-lifecycle mechanics with no meaning outside ScannerPage.
Nothing else in the app needs to observe zoom level. Do not route this through a
ViewModel or introduce `[ObservableProperty]` for it.

## Prerequisites

- `CameraBarcodeReaderView` already declared in `ScannerPage.xaml`.
- ZXing.Net.MAUI version that exposes `ZoomFactor` (normalized `0f`–`1f`; confirm
  against the installed package — this was not always present, see the ZXing
  how-to guide's version notes).

## Required fields in `ScannerPage.xaml.cs`

```csharp
private const float MinZoomRatio = 1f;   // 1.0x — matches CameraX's physical floor
private const float MaxZoomRatio = 5f;   // UI-chosen ceiling. NOT read from device
                                          // hardware — ZXing.Net.MAUI does not expose
                                          // the device's real max zoom ratio publicly.
                                          // Actual device max may be higher or lower;
                                          // the slider/pinch will plateau at whatever
                                          // the hardware supports even if this says 5x.

private float _zoomRatio = MinZoomRatio;      // current magnification, 1.0 = 1x
private float _startZoomRatio = MinZoomRatio; // frozen at gesture start, see below
```

## XAML

```xml
<zxing:CameraBarcodeReaderView
    x:Name="cameraBarcodeReaderView"
    BarcodesDetected="BarcodesDetected">
    <zxing:CameraBarcodeReaderView.GestureRecognizers>
        <PinchGestureRecognizer PinchUpdated="OnPinchUpdated" />
    </zxing:CameraBarcodeReaderView.GestureRecognizers>
</zxing:CameraBarcodeReaderView>

<Label x:Name="ZoomLabel" Text="1.0x" />
```

## Code-behind

```csharp
private void OnPinchUpdated(object? sender, PinchGestureUpdatedEventArgs e)
{
    switch (e.Status)
    {
        case GestureStatus.Started:
            // Freeze the ratio we were at when this gesture began. Do NOT
            // reassign this every Running frame — it must stay fixed for the
            // duration of one pinch gesture.
            _startZoomRatio = _zoomRatio;
            break;

        case GestureStatus.Running:
            // e.Scale is relative to the PREVIOUS callback, not gesture start
            // (confirmed in MS docs for PinchGestureUpdatedEventArgs.Scale).
            // Accumulate against the frozen starting ratio, matching the
            // official PinchToZoomContainer pattern from MS Learn.
            var candidate = _zoomRatio + (float)(e.Scale - 1.0) * _startZoomRatio;
            ApplyZoomRatio(candidate);
            break;

        // No action needed on Completed — _zoomRatio already holds the final value.
    }
}

private void ApplyZoomRatio(float ratio)
{
    _zoomRatio = Math.Clamp(ratio, MinZoomRatio, MaxZoomRatio);

    ZoomLabel.Text = $"{_zoomRatio:0.0}x";

    var normalized = (_zoomRatio - MinZoomRatio) / (MaxZoomRatio - MinZoomRatio);
    cameraBarcodeReaderView.ZoomFactor = Math.Clamp(normalized, 0f, 1f);
}
```

## Working solution

I had problems with guids - above - implementation. But the following solution works.

The implementation currently used by `ScannerPage` differs from the ratio-based
example above. It works directly with ZXing's normalized `ZoomFactor`: `0` is
minimum zoom and `1` is maximum zoom. The HUD displays that same normalized
value; it is not a physical magnification multiplier.

In this implementation, a pinch produces one `GestureStatus.Started` event
followed by many `GestureStatus.Running` events. Do not freeze the zoom value
at `Started` and calculate each running update from that frozen value: that
approach prevented zoom from changing in this app. Instead, add each running
event's scale delta to the current zoom value, then clamp it to `[0, 1]`.

The transparent `Grid` over the camera preview hosts the recognizer. This
overlay is used because it captures the pinch reliably in this page:

```xml
<Grid Grid.Row="0" Grid.Column="0" BackgroundColor="Transparent">
    <Grid.GestureRecognizers>
        <PinchGestureRecognizer PinchUpdated="OnPinchUpdated" />
    </Grid.GestureRecognizers>
</Grid>
```

Initialize the page's current value from the camera control when the page
appears:

```csharp
private float _currentZoomFactor;
private const float _maxZoomF = 1f;
private const float _minZoomF = 0f;
private const float _sensitivity = 0.5f;

// In OnAppearing:
_currentZoomFactor = CameraBarcodeReaderView?.ZoomFactor ?? 0f;
```

Apply each `Running` event incrementally. The example below shows the zoom
calculation; keep the camera-control assignment and UI updates on the main
thread, as in `ScannerPage`:

```csharp
if (e.Status == GestureStatus.Running)
{
    var delta = (float)(e.Scale - 1.0) * _sensitivity;
    _currentZoomFactor = Math.Clamp(
        _currentZoomFactor + delta,
        _minZoomF,
        _maxZoomF);

    MainThread.BeginInvokeOnMainThread(() =>
    {
        CameraBarcodeReaderView.ZoomFactor = _currentZoomFactor;

        var displayValue = Math.Round(_currentZoomFactor, 3);
        _viewModel.StatusMessage = $"Zoom: {displayValue:0.###}";
        ZoomHudLabel.Text = $"Zoom: {displayValue:0.###}";
        ZoomHud.IsVisible = true;
    });
}
```

On `Completed` or `Canceled`, the current implementation hides the HUD after
a short delay and restores the default status message. There are no preset
zoom buttons. This section records the working project implementation; use it
instead of the ratio-based calculation above when making changes to this app.

## Behavior at the edges

- **Zoom out below 1.0x is not possible.** `MinZoomRatio = 1f` is the camera's
  physical floor (no such thing as "0x"). Pinching in below this clamps at 1.0x —
  this is expected, not a bug.
- **Zoom in above `MaxZoomRatio` clamps, and may plateau earlier than the label
  suggests** if the physical device's real max zoom ratio is below the chosen
  `MaxZoomRatio` constant. This is a known limitation of not having the real
  hardware value available (see comment above).

## Optional: preset buttons

```xml
<Button Text="1x" Clicked="OnZoomPresetClicked" />
<Button Text="2x" Clicked="OnZoomPresetClicked" />
<Button Text="3x" Clicked="OnZoomPresetClicked" />
```

```csharp
private void OnZoomPresetClicked(object? sender, EventArgs e)
{
    if (sender is Button { Text: string text } &&
        float.TryParse(text.TrimEnd('x'), out var target))
    {
        ApplyZoomRatio(target);
    }
}
```

Same caveat applies: a "3x" button will silently land below 3x on a device
whose real max is lower.

## Do NOT

- Do NOT use a ViewModel, `[ObservableProperty]`, or data binding for zoom state —
  this is page-local gesture mechanics, not app state.
- Do NOT recompute `_startZoomRatio` on every `Running` frame — only on `Started`.
- Do NOT treat `MaxZoomRatio = 5f` as a verified hardware value in comments or
  documentation — it is a UI convention, not a queried device capability.
- Do NOT display zoom as a percentage — magnification ("x") requires no
  conversion and matches the ratio CameraX/ZXing already operate in.
- Do NOT attribute implementation choices to "ZXing guidance" or similar unless
  the source is an actual ZXing.Net.MAUI doc or source file — ZXing.Net.MAUI has
  no gesture-handling API surface; pinch mechanics come from .NET MAUI itself.