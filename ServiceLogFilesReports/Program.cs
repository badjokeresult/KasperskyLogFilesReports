using ServiceLogFilesReports.Workers.Abstractions;
using ServiceLogFilesReports.Workers.Implementations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddMvc();

builder.Services.AddScoped<IReportBuilder, LogFileReportBuilder>();
builder.Services.AddScoped<IFilesWorker, LogFilesWorker>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();