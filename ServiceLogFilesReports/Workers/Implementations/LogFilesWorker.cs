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

    public string GetLineWithAnonymizedEmail(LogLine line)
    {
        var pattern = new Regex(
            @"^((([0-9A-Za-z]{1}[-0-9A-z\.]{0,30}[0-9A-Za-z]?)|([0-9А-Яа-я]{1}[-0-9А-я\.]{0,30}[0-9А-Яа-я]?))@([-A-Za-z]{1,}\.){1,}[-A-Za-z]{2,})$");

        var text = line.LineDescription.Split(' ');
        var textBuilder = new StringBuilder();
        foreach (var word in text)
        {
            if (pattern.IsMatch(word))
            {
                for (var i = 0; i < word.Length; i++)
                {
                    if (word[i] == '@')
                        break;

                    if (i % 2 != 0)
                        textBuilder.Append('*');
                    else
                        textBuilder.Append(word[i]);
                }
            }
            else
            {
                textBuilder.Append(word);
            }
        }

        return textBuilder.ToString();
    }

    public async Task WriteLinesToFilesAsync(
        string path,
        string serviceName,
        IEnumerable<LogLine> logLines,
        int maxFileSize,
        int maxRotationsAmount)
    {
        await using var file = new FileStream($"{serviceName}.log", FileMode.OpenOrCreate, FileAccess.ReadWrite);

        var lineBuilder = new StringBuilder();
        foreach (var line in logLines)
        {
            lineBuilder.Append(GetLineWithAnonymizedEmail(line));
            
            var buffer = Encoding.ASCII.GetBytes(lineBuilder.ToString());
            await file.WriteAsync(buffer, 0, buffer.Length);

            lineBuilder.Clear();

            if (file.Length >= maxFileSize)
                ProvideFilesRotation(path, serviceName);

            if (IsMaxAmountOfRotations(path, serviceName, maxRotationsAmount))
                throw new OverflowException();
        }
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
            .Where(f => f.Contains(serviceName))
            .OrderBy(f => f)
            .SkipLast(1)
            .TakeLast(1)
            .First();

        var possibleNumberOfRotations = file.Split('.').Skip(1).Take(1).FirstOrDefault();
        if (possibleNumberOfRotations != null)
        {
            int currentNumberOfRotations;
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
            if (file.Contains(serviceName))
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
        foreach (var file in Directory.GetFiles(path))
        {
            if (file.Contains(serviceName))
            {
                var lines = await File.ReadAllLinesAsync(file);
                var flag = false;
                foreach (var logLine in logLines)
                {
                    for (var i = 0; i < lines.Length; i++)
                    {
                        var lineDateTime = DateTime.Parse(lines[i].Split('[', ']').First());
                        if (logLine.LineDateTime == lineDateTime)
                        {
                            lines[i] = $"[{lineDateTime}][{logLine.LineCategoryName}] {logLine.LineDescription}\n";
                            flag = true;
                        }
                    }
                }

                if (flag)
                {
                    File.Delete(file);
                    await File.WriteAllLinesAsync(file, lines);
                }
            }
        }
    }

    public async Task DeleteLinesFromFilesAsync(string path, string serviceName, IEnumerable<LogLine> logLines)
    {
        foreach (var file in Directory.GetFiles(path))
        {
            if (file.Contains(serviceName))
            {
                var lines = await File.ReadAllLinesAsync(file);
                var flag = false;
                foreach (var logLine in logLines)
                {
                    for (var i = 0; i < lines.Length; i++)
                    {
                        var lineDateTime = DateTime.Parse(lines[i].Split('[', ']').First());
                        if (logLine.LineDateTime == lineDateTime)
                        {
                            lines[i] = string.Empty;
                            flag = true;
                        }
                    }
                }

                if (flag)
                {
                    lines = lines
                        .Where(l => !l.Equals(string.Empty))
                        .ToArray();

                    File.Delete(file);
                    await File.WriteAllLinesAsync(file, lines);
                }
            }
        }
    }
}