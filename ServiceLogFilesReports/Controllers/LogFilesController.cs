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
        foreach (var line in logLines)
        {
            await _filesWorker.WriteLineToFileAsync(path, serviceName, line, maxFileSize, maxRotationsAmount);
        }

        foreach (var file in Directory.GetFiles(path))
        { 
            var lines = await System.IO.File.ReadAllLinesAsync(file);
            var linesWithAnonymizedEmails = new List<string>();
            foreach (var line in lines)
            {
                linesWithAnonymizedEmails.Add(_filesWorker.GetAnonymizedEmailInLine(line));
            }
            
            await System.IO.File.WriteAllLinesAsync(file, linesWithAnonymizedEmails);
        }
        
        return Ok(new {count = logLines.ToList().Count});
    }

    [HttpPut]
    [Route("logs")]
    public async Task<IActionResult> PutLogsAsync(
        [FromQuery] string path,
        [FromQuery] string serviceName,
        [FromBody] IEnumerable<LogLine> logLines)
    {
        throw new NotImplementedException();
    }

    [HttpDelete]
    [Route("logs")]
    public async Task<IActionResult> DeleteLogsAsync([FromQuery] string path, [FromBody] List<IFormFile> files)
    {
        throw new NotImplementedException();
    }
}