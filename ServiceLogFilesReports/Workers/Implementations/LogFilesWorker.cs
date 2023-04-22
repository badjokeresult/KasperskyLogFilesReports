using System.Text;
using System.Text.RegularExpressions;
using ServiceLogFilesReports.Entities;
using ServiceLogFilesReports.Workers.Abstractions;

namespace ServiceLogFilesReports.Workers.Implementations;

public class LogFilesWorker : IFilesWorker
{
    public IEnumerable<string> GetListOfFiles(Regex pattern, string path)
    {
        var files = new List<string>();

        foreach (var filePath in Directory.GetFiles(path))
        {
            var file = filePath.Split('/').Last();
            if (pattern.IsMatch(file))
                files.Add(path + "/" + file);
        }

        return files;
    }

    public Dictionary<string, IList<string>> GroupFilesByNames(IEnumerable<string> files)
    {
        var filesByNames = new Dictionary<string, IList<string>>();

        foreach (var file in files)
        {
            var name = file.Split('.').First();
            if (!filesByNames.ContainsKey(name))
                filesByNames[name] = new List<string>();
            filesByNames[name].Add(file);
        }

        return filesByNames;
    }

    public string GetAnonymizedEmailInLine(string line)
    {
        var pattern = new Regex(
            @"^((([0-9A-Za-z]{1}[-0-9A-z\.]{0,30}[0-9A-Za-z]?)|([0-9А-Яа-я]{1}[-0-9А-я\.]{0,30}[0-9А-Яа-я]?))@([-A-Za-z]{1,}\.){1,}[-A-Za-z]{2,})$");

        var words = line.Split(' ');
        var anonymizedEmail = new StringBuilder();
        foreach (var word in words)
        {
            if (pattern.IsMatch(word))
            {
                for (var i = 0; i < word.Length; i++)
                {
                    if (word[i] == '@')
                        break;

                    if (i % 2 != 0)
                        anonymizedEmail.Append('*');
                    else
                        anonymizedEmail.Append(word[i]);
                }
            }
        }

        return anonymizedEmail.ToString();
    }

    public async Task WriteLineToFileAsync(
        string path,
        string serviceName,
        LogLine logLine,
        int maxFileSize,
        int maxRotationsAmount)
    {
        await using var file = new FileStream($"{serviceName}.log", FileMode.OpenOrCreate, FileAccess.ReadWrite);

        var buffer = Encoding.ASCII.GetBytes(logLine.ToString());
        await file.WriteAsync(buffer, 0, buffer.Length);
        
        if (file.Length >= maxFileSize)
            ProvideFilesRotation(path, serviceName);

        if (IsMaxAmountOfRotations(path, serviceName, maxRotationsAmount))
            throw new OverflowException();
    }

    private void ProvideFilesRotation(string path, string serviceName)
    {
        foreach (var file in Directory.GetFiles(path))
        {
            var splittedFileName = file.Split('/').Last().Split('.');
            var fileName = splittedFileName[0];
            if (fileName == serviceName)
            {
                int number;
                if (int.TryParse(splittedFileName[1], out number))
                    File.Move(string.Join(' ', splittedFileName), $"{serviceName}.{number + 1}.log");
                else
                    File.Move(string.Join(' ', splittedFileName), $"{serviceName}.1.log");
            }
        }
    }

    private bool IsMaxAmountOfRotations(string path, string serviceName, int maxValue)
    {
        var file = Directory.GetFiles(path)
            .Where(f => f.Split('/').Last().Split('.').First() == serviceName)
            .OrderBy(f => f)
            .SkipLast(1)
            .TakeLast(1)
            .First();

        var possibleNumberOfRotations = file.Split('.').Skip(1).Take(1).FirstOrDefault();
        int currentNumberOfRotations;
        if (possibleNumberOfRotations != null)
        {
            if (int.TryParse(possibleNumberOfRotations, out currentNumberOfRotations))
            {
                return currentNumberOfRotations >= maxValue;
            }
        }

        return false;
    }

    public async Task<IEnumerable<string>> ReadAllLinesFromFilesAsync(string path, string serviceName)
    {
        var lines = new List<string>();
        
        foreach (var file in Directory.GetFiles(path))
        {
            if (file.Split('/').Last().Split('.').First() == serviceName)
            {
                foreach (var line in await File.ReadAllLinesAsync(file))
                {
                    lines.Add(line);
                }
            }
        }

        return lines;
    }

    public async Task UpdateLinesInFilesAsync(string path, string serviceName, IEnumerable<LogLine> logLines)
    {
        // TODO: write new file with temp name, write new and old lines into it, then remove an old file and rename the temp one
    }
}