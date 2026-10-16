using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace UIApp.Services
{
    // Pure, static parsers for QR payloads used in Phase 2. No platform calls here to keep unit testing simple.
    public static class PayloadParsers
    {
        // WIFI: WIFI:T:WPA;S:SSID;P:PASSWORD;H:true;;
        public static WifiPayload? ParseWifi(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            if (!text.StartsWith("WIFI:", StringComparison.OrdinalIgnoreCase)) return null;

            // Remove trailing ;; if present
            var payload = text.Substring(5);

            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // Split on semicolons but ignore empty final
            var parts = payload.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var p in parts)
            {
                var idx = p.IndexOf(':');
                if (idx <= 0) continue;
                var k = p.Substring(0, idx).Trim();
                var v = p.Substring(idx + 1).Trim();
                map[k] = v;
            }

            if (!map.ContainsKey("S")) return null; // SSID required

            var wifi = new WifiPayload
            {
                Ssid = map.TryGetValue("S", out var s) ? s : string.Empty,
                Password = map.TryGetValue("P", out var pword) ? pword : string.Empty,
                AuthenticationType = map.TryGetValue("T", out var t) ? t : "",
                Hidden = map.TryGetValue("H", out var h) && (h.Equals("true", StringComparison.OrdinalIgnoreCase) || h == "1")
            };

            return wifi;
        }

        // vCard: BEGIN:VCARD...END:VCARD (simple parser for common fields)
        public static VCardPayload? ParseVCard(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            if (!text.Contains("BEGIN:VCARD", StringComparison.OrdinalIgnoreCase)) return null;

            var lines = Regex.Split(text, "\r?\n");

            var v = new VCardPayload();

            foreach (var raw in lines)
            {
                if (string.IsNullOrWhiteSpace(raw)) continue;
                var line = raw.Trim();
                if (line.StartsWith("FN:", StringComparison.OrdinalIgnoreCase))
                {
                    v.FullName = line.Substring(3).Trim();
                }
                else if (line.StartsWith("N:", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(v.FullName))
                {
                    // N:Last;First;Middle;Prefix;Suffix
                    var parts = line.Substring(2).Split(';');
                    v.LastName = parts.ElementAtOrDefault(0)?.Trim();
                    v.FirstName = parts.ElementAtOrDefault(1)?.Trim();
                }
                else if (line.StartsWith("TEL", StringComparison.OrdinalIgnoreCase))
                {
                    var idx = line.IndexOf(':');
                    if (idx > 0)
                    {
                        var tel = line.Substring(idx + 1).Trim();
                        v.PhoneNumbers.Add(tel);
                    }
                }
                else if (line.StartsWith("EMAIL", StringComparison.OrdinalIgnoreCase))
                {
                    var idx = line.IndexOf(':');
                    if (idx > 0)
                    {
                        var mail = line.Substring(idx + 1).Trim();
                        v.Emails.Add(mail);
                    }
                }
                else if (line.StartsWith("ORG:", StringComparison.OrdinalIgnoreCase))
                {
                    v.Organization = line.Substring(4).Trim();
                }
            }

            // If no meaningful fields found, return null
            if (string.IsNullOrWhiteSpace(v.FullName) && !v.Emails.Any() && !v.PhoneNumbers.Any())
                return null;

            return v;
        }

        // VEVENT / iCal simplified parser. Looks for BEGIN:VEVENT ... END:VEVENT and extracts SUMMARY, DTSTART, DTEND, LOCATION, DESCRIPTION
        public static CalendarEventPayload? ParseVEvent(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            if (!text.Contains("BEGIN:VEVENT", StringComparison.OrdinalIgnoreCase)) return null;

            var lines = Regex.Split(text, "\r?\n");
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string? currentKey = null;
            foreach (var raw in lines)
            {
                if (string.IsNullOrWhiteSpace(raw)) continue;
                var line = raw.Trim();
                // Folded lines (continuation) start with space or tab — append to previous value
                if ((line.StartsWith(" ") || line.StartsWith("\t")) && currentKey != null)
                {
                    dict[currentKey] = dict[currentKey] + line.Trim();
                    continue;
                }

                var idx = line.IndexOf(':');
                if (idx <= 0) continue;
                var key = line.Substring(0, idx);
                var val = line.Substring(idx + 1);
                currentKey = key;
                dict[key] = val;
            }

            if (!dict.Any()) return null;

            var ev = new CalendarEventPayload();
            if (dict.TryGetValue("SUMMARY", out var summary)) ev.Summary = summary.Trim();
            if (dict.TryGetValue("DTSTART", out var dtstart)) ev.Start = ParseDateTime(dtstart.Trim());
            if (dict.TryGetValue("DTEND", out var dtend)) ev.End = ParseDateTime(dtend.Trim());
            if (dict.TryGetValue("LOCATION", out var loc)) ev.Location = loc.Trim();
            if (dict.TryGetValue("DESCRIPTION", out var desc)) ev.Description = desc.Trim();

            if (string.IsNullOrWhiteSpace(ev.Summary) && ev.Start == null)
                return null;

            return ev;
        }

        private static DateTimeOffset? ParseDateTime(string raw)
        {
            // Try multiple common iCal formats: YYYYMMDDTHHMMSSZ, YYYYMMDD
            if (string.IsNullOrWhiteSpace(raw)) return null;
            // If ends with Z, treat as UTC
            if (DateTimeOffset.TryParseExact(raw, "yyyyMMdd'T'HHmmss'Z'", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dto))
                return dto;
            if (DateTimeOffset.TryParseExact(raw, "yyyyMMdd'T'HHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dto))
                return dto;
            if (DateTimeOffset.TryParseExact(raw, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dtd))
                return dtd;

            // Fallback to generic parse
            if (DateTimeOffset.TryParse(raw, out var any)) return any;
            return null;
        }
    }

    // Payload DTOs
    public class WifiPayload
    {
        public string Ssid { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string AuthenticationType { get; set; } = string.Empty; // e.g., WEP/WPA
        public bool Hidden { get; set; }
    }

    public class VCardPayload
    {
        public string? FullName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Organization { get; set; }
        public List<string> PhoneNumbers { get; set; } = new List<string>();
        public List<string> Emails { get; set; } = new List<string>();
    }

    public class CalendarEventPayload
    {
        public string? Summary { get; set; }
        public DateTimeOffset? Start { get; set; }
        public DateTimeOffset? End { get; set; }
        public string? Location { get; set; }
        public string? Description { get; set; }
    }
}
