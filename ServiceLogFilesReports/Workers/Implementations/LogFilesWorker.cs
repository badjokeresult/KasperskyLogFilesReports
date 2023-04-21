using System.Net;
using System.Text.RegularExpressions;
using ServiceLogFilesReports.Workers.Abstractions;

namespace ServiceLogFilesReports.Workers.Implementations;

public class LogFilesWorker : IFilesWorker
{
    public IEnumerable<string> GetListOfFiles(Regex pattern, string path)
    {
        var fullPath = GetAbsolutePathToFilesDir(path);

        var files = new List<string>();

        foreach (var filePath in Directory.GetFiles(fullPath))
        {
            var file = filePath.Split('/').Last();
            if (pattern.IsMatch(file))
                files.Add(file);
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

    public void AddLineToFile(string line, string file, int? maxFileSize = null, int? maxRotationsAmount = null)
    {
        if (!File.Exists(file))
        {
            WriteToNewFile(line, file, maxFileSize, maxRotationsAmount);
            ProvideFilesRotation();
        }

        throw new NotImplementedException();
    }

    private void WriteToNewFile(string line, string file, int? maxFileSize = null, int? maxRotationsAmount = null)
    {
        throw new NotImplementedException();
    }

    private void ProvideFilesRotation()
    {
        throw new NotImplementedException();
    }

    /*private string GetLineWithAnonimyzedEmailIfThere(string line)
    {
        var pattern = new Regex(
            @"^((([0-9A-Za-z]{1}[-0-9A-z\.]{0,30}[0-9A-Za-z]?)|([0-9А-Яа-я]{1}[-0-9А-я\.]{0,30}[0-9А-Яа-я]?))@([-A-Za-z]{1,}\.){1,}[-A-Za-z]{2,})$");

        var text = line.Split(' ').Last();
        var words = text.Split(' ');
        
        var email = string.Empty;
        foreach (var word in words)
        {
            if (pattern.IsMatch(word))
            {
                email = word;
                break;
            }
        }
        
        
    }*/

    private string GetAbsolutePathToFilesDir(string path)
    {
        var absolutePathPattern = new Regex(@"(/)|(\w:)|(~)");

        if (absolutePathPattern.IsMatch(path.Substring(0, 2)))
            return path;
        return Environment.CurrentDirectory + "/" + path;
    }
}