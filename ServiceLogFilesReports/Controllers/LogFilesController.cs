using Microsoft.AspNetCore.Mvc;

using ServiceLogFilesReports.Entities;
using ServiceLogFilesReports.Workers.Abstractions;

namespace ServiceLogFilesReports.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class LogFilesController : Controller
{
    private readonly IFilesWorker _filesWorker;

    public LogFilesController(IFilesWorker filesWorker)
    {
        _filesWorker = filesWorker;
    }
    
    [HttpGet]
    [Route("logs")]
    public async Task<IActionResult> GetLogsAsync([FromQuery] string path, [FromQuery] string serviceName)
    {
        var logs = await _filesWorker.ReadAllLinesFromFilesAsync(path, serviceName);
        
        if (logs.ToList().Count > 0)
            return new JsonResult(logs);
        return NotFound();
    }

    [HttpPost]
    [Route("logs")]
    public async Task<IActionResult> PostLogsAsync(
        [FromQuery] string path,
        [FromQuery] string serviceName,
        [FromQuery] int maxFileSize,
        [FromQuery] int maxRotationsAmount,
        [FromBody] IEnumerable<LogLine> logLines)
    {
        await _filesWorker.WriteLinesToFilesAsync(path, serviceName, logLines, maxFileSize, maxRotationsAmount);

        return Ok(new {count = logLines.ToList().Count});
    }

    [HttpPut]
    [Route("logs")]
    public async Task<IActionResult> PutLogsAsync(
        [FromQuery] string path,
        [FromQuery] string serviceName,
        [FromBody] IEnumerable<LogLine> logLines)
    {
        await _filesWorker.UpdateLinesInFilesAsync(path, serviceName, logLines);

        return Ok(new {count = logLines.ToList().Count});
    }

    [HttpDelete]
    [Route("logs")]
    public async Task<IActionResult> DeleteLogsAsync(
        [FromQuery] string path,
        [FromQuery] string serviceName,
        [FromBody] IEnumerable<LogLine> logLines)
    {
        await _filesWorker.DeleteLinesFromFilesAsync(path, serviceName, logLines);

        return Ok(new {count = logLines.ToList().Count});
    }
}