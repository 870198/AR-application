using SQLite;

namespace ConelARClean;

public static class DetectionReader
{
    private static readonly Lazy<SQLiteConnection> _conn = new(() =>
    {
        var path = System.IO.Path.Combine(
            Microsoft.Maui.Storage.FileSystem.AppDataDirectory,
            "detections.db3");

        return new SQLiteConnection(path, true);
    });

    public static List<DetectionEvent> GetAll()
    {
        try
        {
            return _conn.Value.Table<DetectionEvent>()
                .OrderByDescending(x => x.Timestamp)
                .ToList();
        }
        catch
        {
            return new List<DetectionEvent>();
        }
    }
}