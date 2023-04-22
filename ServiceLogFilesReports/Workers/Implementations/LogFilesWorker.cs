using System.Text;
using System.Text.RegularExpressions;

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
}