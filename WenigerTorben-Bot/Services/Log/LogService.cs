using System;
using System.IO;
using Serilog;
using WenigerTorbenBot.Services.File;

namespace WenigerTorbenBot.Services.Log;

public class LogService : Service, ILogService, IDisposable
{
    public override string Name => "Log";
    public override ServicePriority Priority => ServicePriority.Essential;

    private readonly IFileService fileService;

    public LogService(IFileService fileService)
    {
        this.fileService = fileService;
    }

    protected override void Initialize()
    {
        string loggingDirectory = fileService.GetAndCreateDirectory("Logs");
        string logfilePath = Path.Combine(loggingDirectory, $"{DateTime.Now:yyyy-MM-dd-mm-ss}.log");

        Serilog.Log.Logger = new LoggerConfiguration()
                                .WriteTo.Console()
                                .WriteTo.File(logfilePath)
                                .CreateLogger();

        Serilog.Log.Information("Logger initialized");
        Serilog.Log.Information("Logging directory is {loggingDirectory}", loggingDirectory);
    }

    public void Dispose() => Serilog.Log.CloseAndFlush();

    protected override ServiceConfiguration CreateServiceConfiguration() => new ServiceConfigurationBuilder().Build();

}