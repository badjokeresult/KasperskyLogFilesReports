using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Mvc;

using ServiceLogFilesReports.Entities;

namespace ServiceLogFilesReports.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class LogFileReportsController
{
    [HttpGet("get-reports-by-regex-string")]
    public async Task<IActionResult> GetAsync([FromQuery] string serviceName)
    {
        var regex = new Regex(serviceName);

        var filesByServices = GetListsOfLogs(regex);

        var reports = new List<LogFileReport>();
        foreach (var service in filesByServices.Keys)
            reports.Add(BuildReport(filesByServices[service]));

        return OkResult(reports);
    }

    private Dictionary<string, List<string>> GetListsOfLogs(Regex regex)
    {
        var filesByServices = new Dictionary<string, List<string>>();

        foreach (var path in Directory.GetFiles(Environment.CurrentDirectory))
        {
            var file = path.Split('/').Last();
            if (regex.IsMatch(file))
            {
                var fileNameParts = file.Split('.');
                if (!filesByServices.ContainsKey(fileNameParts[0]))
                    filesByServices[fileNameParts[0]] = new List<string>();
                filesByServices[fileNameParts[0]].Add(file);
            }
        }

        return filesByServices;
    }

    private LogFileReport BuildReport(List<string> logFiles)
    {
        var report = new LogFileReport();

        report.ServiceName = GetServiceName(logFiles);
        report.EarliestLogLine = GetEarliestLogLineDateTime(logFiles);
        report.NewestLogLine = GetNewestLogLineDateTime(logFiles);
        report.CategoriesCounts = GetAmountOfLinesByCategories(logFiles);
        report.RotationsAmount = GetAmountOfRotations(logFiles);

        return report;
    }

    private string GetServiceName(List<string> logFiles) => 
        logFiles[0].Split('.')[0];

    private DateTime GetEarliestLogLineDateTime(List<string> logFiles)
    {
        var fileName = logFiles
            .OrderByDescending(f => f)
            .First();

        using var file = new StreamReader(fileName);

        var line = file.ReadLine()!;

        return GetDateTimeFromLogLine(line);
    }

    private DateTime GetNewestLogLineDateTime(List<string> logFiles)
    {
        var fileName = logFiles
            .OrderBy(f => f)
            .First();

        using var file = new StreamReader(fileName);

        var line = string.Empty;
        while (!file.EndOfStream)
        {
            line = file.ReadLine();
        }

        return GetDateTimeFromLogLine(line!);
    }

    private Dictionary<string, int> GetAmountOfLinesByCategories(List<string> logFiles)
    {
        var linesAmountsByCategories = new Dictionary<string, int>();

        foreach (var fileName in logFiles)
        {
            foreach (var line in File.ReadLines(fileName))
            {
                var category = GetParsedLogLine(line)[1];
                if (!linesAmountsByCategories.ContainsKey(category))
                    linesAmountsByCategories[category] = 0;
                linesAmountsByCategories[category]++;
            }
        }

        return linesAmountsByCategories;
    }

    private DateTime GetDateTimeFromLogLine(string line)
    {
        var parsedLine = GetParsedLogLine(line);
        
        return DateTime.Parse(parsedLine[0]);
    }

    private string[] GetParsedLogLine(string line) => 
        line.Split('[', ']')
            .Select(s => s.Trim())
            .Where(s => s != "")
            .ToArray();

    private string GetCategoryFromLogLine(string line)
    {
        var parsedLine = GetParsedLogLine(line);

        return parsedLine[1];
    }

    private int GetAmountOfRotations(List<string> logFiles)
    {
        var oldestFile = logFiles
            .OrderByDescending(f => f)
            .First();

        return int.Parse(oldestFile.Split('.')[1]);
    }
}