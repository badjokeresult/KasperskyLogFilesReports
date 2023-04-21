using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Mvc;

using ServiceLogFilesReports.Entities;
using ServiceLogFilesReports.Workers.Abstractions;

namespace ServiceLogFilesReports.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class LogFileReportsController : Controller
{
    private readonly IReportBuilder _reportBuilder;
    private readonly IFilesWorker _filesWorker;

    public LogFileReportsController(IReportBuilder reportBuilder, IFilesWorker filesWorker)
    {
        _reportBuilder = reportBuilder;
        _filesWorker = filesWorker;
    }
    
    [HttpGet]
    [Route("report")]
    public async Task<IActionResult> GetAsync([FromBody] string serviceName, [FromBody] string path)
    {
        var regex = new Regex(serviceName);

        var files = _filesWorker.GetListOfFiles(regex, path);

        var filesByServices = _filesWorker.GroupFilesByNames(files);

        var reports = new List<LogFileReport>();
        foreach (var service in filesByServices.Keys)
        {
            var report = _reportBuilder.BuildReport(filesByServices[service]);
            reports.Add(report);
        }

        return new JsonResult(reports);
    }

    [HttpPost]
    [Route("log")]
    public async Task<IActionResult> PostAsync(
        [FromBody] string path,
        [FromBody] string serviceName,
        [FromBody] string logLine,
        [FromBody] int? maxLogFileSize = null,
        [FromBody] int? maxRotationsAmount = null)
    {
        throw new NotImplementedException();
    }

    [HttpPut]
    [Route("log")]
    public async Task<IActionResult> PutAsync()
    {
        throw new NotImplementedException();
    }

    [HttpDelete]
    [Route("log")]
    public async Task<IActionResult> DeleteAsync()
    {
        throw new NotImplementedException();
    }
}