using ServiceLogFilesReports.Entities;
using ServiceLogFilesReports.Workers.Abstractions;

namespace ServiceLogFilesReports.Workers.Implementations;

public class LogFileReportBuilder : IReportBuilder
{
    public LogFileReport BuildReport(IEnumerable<string> files)
    {
        var report = new LogFileReport();
        
        

        report.ServiceName = GetServiceName(files);
        report.EarliestLogLine = GetEarliestLogLineDateTime(files);
        report.NewestLogLine = GetNewestLogLineDateTime(files);
        report.CategoriesCounts = GetAmountOfLinesByCategories(files);
        report.RotationsAmount = GetAmountOfRotations(files);

        return report;
    }

    private string GetServiceName(IEnumerable<string> files) => 
        files.First().Split('.')[0];

    private DateTime GetEarliestLogLineDateTime(IEnumerable<string> files)
    {
        var fileName = files
            .OrderBy(f => f)
            .SkipLast(1)
            .Last();
        
        Console.WriteLine($"*** *** *** {nameof(GetEarliestLogLineDateTime)}, FileName: {fileName}");

        using var file = new StreamReader(fileName);

        var line = file.ReadLine();

        return GetDateTimeFromLogLine(line!);
    }

    private DateTime GetNewestLogLineDateTime(IEnumerable<string> files)
    {
        var fileName = files
            .OrderBy(f => f)
            .Last();

        using var file = new StreamReader(fileName);

        var line = string.Empty;
        while (!file.EndOfStream)
        {
            line = file.ReadLine();
        }

        return GetDateTimeFromLogLine(line!);
    }

    private Dictionary<string, int> GetAmountOfLinesByCategories(IEnumerable<string> files)
    {
        var linesAmountsByCategories = new Dictionary<string, int>();

        foreach (var fileName in files)
        {
            using var file = new StreamReader(fileName);
            while (!file.EndOfStream)
            {
                var line = file.ReadLine();
                var category = GetParsedLogLine(line!).Skip(1).First();
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

        return DateTime.Parse(parsedLine.First());
    }

    private IEnumerable<string> GetParsedLogLine(string line) => 
        line.Split('[', ']')
            .Select(s => s.Trim())
            .Where(s => s != "")
            .ToArray();

    private int GetAmountOfRotations(IEnumerable<string> files)
    {
        var oldestFileNameParts = files
            .OrderBy(f => f)
            .SkipLast(1)
            .Last()
            .Split('.');

        if (oldestFileNameParts.Length < 3)
            return 0;
        return int.Parse(oldestFileNameParts[1]);
    }
}