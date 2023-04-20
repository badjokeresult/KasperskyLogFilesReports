namespace ServiceLogFilesReports.Entities;

public class LogFileReport
{
    public string ServiceName { get; set; } = null!;
    public DateTime EarliestLogLine { get; set; }
    public DateTime NewestLogLine { get; set; }
    public Dictionary<string, int> CategoriesCounts { get; set; } = null!;
    public int RotationsAmount { get; set; }

    public override string ToString()
    {
        return base.ToString();
    }
}