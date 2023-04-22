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

    public void AnonymizeEmailsInFiles(string file)
    {
        throw new NotImplementedException();
    }
}