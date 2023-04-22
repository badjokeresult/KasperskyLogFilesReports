using System.Text;

namespace ServiceLogFilesReports.Entities;

public class LogLine
{
    public DateTime LineDateTime { get; set; }
    public string LineCategoryName { get; set; }
    public string LineDescription { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder();
        
        sb.Append($"[{LineDateTime.ToString()}]");
        sb.Append($"[{LineCategoryName}] ");
        sb.Append($"{LineDescription}\n");

        return sb.ToString();
    }
}