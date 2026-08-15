using SQLite;

namespace UIApp.Models
{
    [Table("ScanResults")]
    public class ScanResult
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [MaxLength(2000)]
        public string RawValue { get; set; } = string.Empty;

        // e.g., "URL" or "PlainText"
        public string Type { get; set; } = "PlainText";

        public DateTime ScannedAt { get; set; }

        public bool IsFavorite { get; set; } = false;
    }
}