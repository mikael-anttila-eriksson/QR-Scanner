using SQLite;
using UIApp.Models;

namespace UIApp.Services
{
    public class SqliteStorageService
    {
        const string DbFileName = "scanresults.db3";
        private SQLiteAsyncConnection? _db;

        public async Task InitializeAsync()
        {
            if (_db != null)
                return;

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, DbFileName);
            _db = new SQLiteAsyncConnection(dbPath);
            await _db.CreateTableAsync<ScanResult>();
        }

        public async Task<ScanResult> AddAsync(ScanResult item)
        {
            if (_db == null) await InitializeAsync();
            item.ScannedAt = item.ScannedAt == default ? DateTime.UtcNow : item.ScannedAt;
            await _db!.InsertAsync(item);
            return item;
        }

        public async Task<List<ScanResult>> GetAllAsync()
        {
            if (_db == null) await InitializeAsync();
            var list = await _db!.Table<ScanResult>().OrderByDescending(r => r.ScannedAt).ToListAsync();
            return list;
        }

        public async Task<ScanResult?> GetByIdAsync(int id)
        {
            if (_db == null) await InitializeAsync();
            return await _db!.FindAsync<ScanResult>(id);
        }

        public async Task UpdateAsync(ScanResult item)
        {
            if (_db == null) await InitializeAsync();
            await _db!.UpdateAsync(item);
        }

        public async Task<List<ScanResult>> GetFavoritesAsync()
        {
            if (_db == null) await InitializeAsync();
            var list = await _db!.Table<ScanResult>().Where(r => r.IsFavorite).OrderByDescending(r => r.ScannedAt).ToListAsync();
            return list;
        }

        public async Task ClearAllAsync()
        {
            if (_db == null) await InitializeAsync();
            // Delete all records from ScanResults table
            await _db!.ExecuteAsync("DELETE FROM ScanResults;");
        }

        public async Task DeleteAsync(ScanResult item)
        {
            if (_db == null) await InitializeAsync();
            await _db!.DeleteAsync(item);
        }
    }
}