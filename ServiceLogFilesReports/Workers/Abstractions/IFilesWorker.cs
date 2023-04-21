using System.Text.RegularExpressions;

namespace ServiceLogFilesReports.Workers.Abstractions;

public interface IFilesWorker
{
    public IEnumerable<string> GetListOfFiles(Regex pattern, string path);
    public Dictionary<string, IList<string>> GroupFilesByNames(IEnumerable<string> files);
    public void AddLineToFile(string line, string file, int maxFileSize, int maxRotationsAmount);
}