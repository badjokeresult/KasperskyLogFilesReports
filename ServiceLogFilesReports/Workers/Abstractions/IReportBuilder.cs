using ServiceLogFilesReports.Entities;

namespace ServiceLogFilesReports.Workers.Abstractions;

public interface IReportBuilder
{
    public LogFileReport BuildReport(IEnumerable<string> files);
}