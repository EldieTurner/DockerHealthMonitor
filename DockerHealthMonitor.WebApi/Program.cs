using DockerHealthMonitor.WebApi.Services;

namespace DockerHealthMonitor.WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        try
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure logging: clear default providers and add Console logging.
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.SetMinimumLevel(LogLevel.Information);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Register health checks so the app can report health status.
            builder.Services.AddHealthChecks();

            // Register DockerService for DI.
            builder.Services.AddSingleton<IDockerService, DockerService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();

            // Map controllers.
            app.MapControllers();

            // Map a health check endpoint at /health.
            app.MapHealthChecks("/health");

            app.Run();
        }
        catch (Exception ex)
        {
            // If there's an error during startup, log it as a critical error.
            using var loggerFactory = LoggerFactory.Create(config =>
            {
                config.AddConsole();
            });
            var logger = loggerFactory.CreateLogger<Program>();
            logger.LogCritical(ex, "Application startup failed.");
            throw;
        }
    }
}