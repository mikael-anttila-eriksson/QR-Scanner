using System.Text.RegularExpressions;
using UIApp.Models;

namespace UIApp.Services
{
    /// <summary>
    /// Service for handling barcode scanning logic, debouncing, and classification.
    /// Manages the QR code processing pipeline: validation → classification → storage.
    /// </summary>
    public class ScannerService
    {
        private readonly SqliteStorageService _storage;
        private const double SCAN_DEBOUNCE_MS = 2000;
        private DateTime _lastScanTime = DateTime.MinValue;

        public ScannerService(SqliteStorageService storage)
        {
            _storage = storage;
        }

        /// <summary>
        /// Process a scanned barcode string with debounce and classification.
        /// Returns the saved ScanResult if valid, or null if rejected.
        /// </summary>
        public async Task<ScanResult?> ProcessScannedBarcodeAsync(string rawValue)
        {
            try
            {
                // Validate: reject empty/whitespace
                if (string.IsNullOrWhiteSpace(rawValue))
                    return null;

                // Debounce: ignore scans within 2 seconds of last successful scan
                var timeSinceLastScan = DateTime.Now - _lastScanTime;
                if (timeSinceLastScan.TotalMilliseconds < SCAN_DEBOUNCE_MS)
                    return null;

                _lastScanTime = DateTime.Now;

                // Classify as URL or PlainText
                var type = IsLikelyUrl(rawValue) ? "URL" : "PlainText";

                var scanResult = new ScanResult
                {
                    RawValue = rawValue,
                    Type = type,
                    ScannedAt = DateTime.UtcNow
                };

                await _storage.InitializeAsync();
                await _storage.AddAsync(scanResult);

                return scanResult;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Determine if a string is likely a URL or plaintext.
        /// Uses Uri.IsWellFormedUriString and regex fallback for http/https schemes.
        /// </summary>
        private bool IsLikelyUrl(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            return Uri.IsWellFormedUriString(text, UriKind.Absolute) ||
                   Regex.IsMatch(text, @"^https?://", RegexOptions.IgnoreCase);
        }
    }
}
