using AndroidMedia = global::Android.Media;
using UIApp.Services;

namespace UIApp.Platforms.Android.Services;

public sealed class AndroidSoundService : ISoundService, IDisposable
{
    private const int BeepDurationMilliseconds = 120;
    private const int VolumePercent = 80;

    private readonly object _syncLock = new();
    private AndroidMedia.ToneGenerator? _toneGenerator;
    private bool _disposed;

    public Task PlayShortBeepAsync()
    {
        lock (_syncLock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            _toneGenerator ??= new AndroidMedia.ToneGenerator(AndroidMedia.Stream.Music, VolumePercent);
            if (!_toneGenerator.StartTone(AndroidMedia.Tone.PropBeep, BeepDurationMilliseconds))
            {
                throw new InvalidOperationException("Android could not start the scan beep.");
            }
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        lock (_syncLock)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            if (_toneGenerator is not null)
            {
                _toneGenerator.StopTone();
                _toneGenerator.Release();
                _toneGenerator.Dispose();
                _toneGenerator = null;
            }
        }
    }
}
