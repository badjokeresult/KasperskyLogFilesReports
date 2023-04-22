using Microsoft.AspNetCore.Mvc;
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
        var logs = new List<string>();

        foreach (var file in Directory.GetFiles(path))
        {
            if (file.Split('/').Last().Split('.').First() == serviceName)
                logs.Add(file);
        }

        if (logs.Count > 0)
            return new JsonResult(logs);
        return NotFound();
    }

    [HttpPost]
    [Route("logs")]
    public async Task<IActionResult> PostLogsAsync([FromBody] string path, List<IFormFile> files)
    {
        foreach (var file in files)
        {
            if (file.Length > 0)
            {
                await using var stream = System.IO.File.Create(file.Name);
                await file.CopyToAsync(stream);
            }
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
        
        return Ok(new {count = files.Count});
    }

    [HttpPut]
    [Route("logs")]
    public async Task<IActionResult> PutLogsAsync([FromBody] string path, List<IFormFile> files)
    {
        var opsCount = 0;
        
        foreach (var file in files)
        {
            if (file.Length > 0)
            {
                foreach (var oldFile in Directory.GetFiles(path))
                {
                    if (oldFile.EndsWith(path + file.Name))
                    {
                        System.IO.File.Delete(path + file.Name);
                        await using var stream = System.IO.File.Create(path + file.Name);
                        await file.CopyToAsync(stream);
                        opsCount++;
                    }
                }
            }
        }
        
        return Ok(new {count = opsCount});
    }

    [HttpDelete]
    [Route("logs")]
    public async Task<IActionResult> DeleteLogsAsync([FromBody] string path, List<IFormFile> files)
    {
        var opsCount = 0;
        
        foreach (var file in files)
        {
            foreach (var oldFile in Directory.GetFiles(path))
            {
                if (oldFile.EndsWith(path + file.Name))
                {
                    System.IO.File.Delete(oldFile);
                    opsCount++;
                }
            }
        }

        return Ok(new {count = opsCount});
    }
}