using SQLite;

namespace ConelARClean;

public class DetectionEvent
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string TargetName { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; }
}