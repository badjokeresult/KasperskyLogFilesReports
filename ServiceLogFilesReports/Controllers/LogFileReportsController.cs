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
    [Route("reports")]
    public async Task<IActionResult> GetReportsAsync([FromQuery] string serviceName, [FromQuery] string path)
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
        if (reports.Count > 0)
            return new JsonResult(reports);
        return NotFound();
    }
}