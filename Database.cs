using SQLite;
using System.IO;

namespace StudBudgetMVP.Data;
public static class Database
{
    static SQLiteAsyncConnection _db;
    public static SQLiteAsyncConnection GetConnection()
    {
        if (_db == null)
        {
            var path = Path.Combine(FileSystem.AppDataDirectory, "studbudget.db3");
            _db = new SQLiteAsyncConnection(path);
            _db.CreateTableAsync<User>().Wait();
            _db.CreateTableAsync<Transaction>().Wait();
        }
        return _db;
    }
}