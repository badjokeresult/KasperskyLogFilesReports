using System.Text.RegularExpressions;

using ServiceLogFilesReports.Entities;

namespace ServiceLogFilesReports.Workers.Abstractions;

public interface IFilesWorker
{
    public IEnumerable<string> GetListOfFiles(Regex pattern, string path);
    public Dictionary<string, IList<string>> GroupFilesByNames(IEnumerable<string> files);
    public Task WriteLinesToFilesAsync(
        string path,
        string serviceName,
        IEnumerable<LogLine> logLines,
        int maxFileSize,
        int maxRotationsAmount);
    public Task<IEnumerable<string>> ReadAllLinesFromFilesAsync(string path, string serviceName);
    public Task UpdateLinesInFilesAsync(string path, string serviceName, IEnumerable<LogLine> logLines);
    public Task DeleteLinesFromFilesAsync(string path, string serviceName, IEnumerable<LogLine> logLines);
}