using Docker.DotNet.Models;
using DockerHealthMonitor.WebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DockerHealthMonitor.WebApi.Controllers;

[Route("api/")]
[ApiController]
public class DockerController(IDockerService dockerService, ILogger<DockerController> logger) : ControllerBase
{
    private readonly IDockerService _dockerService = dockerService;
    private readonly ILogger<DockerController> _logger = logger;

    /// <summary>
    /// Gets all Docker containers along with their ID, name, state, and health status (if available).
    /// </summary>
    /// <returns>A list of container summaries.</returns>
    [HttpGet("containers")]
    public async Task<IActionResult> GetContainers()
    {

        _logger.LogInformation("Fetching all containers...");
        var containers = await _dockerService.GetContainersAsync();
        _logger.LogInformation("Fetched {containerCount} containers.", containers.Count());
        var response = PrepareResponse(containers);
        return Ok(response);
    }

    private static List<object> PrepareResponse(IEnumerable<ContainerListResponse> containers)
    {
        var response = new List<object>();

        foreach (var container in containers)
        {
            response.Add(new
            {
                Id = container.ID,
                Name = container.Names.FirstOrDefault()?.TrimStart('/'),
                container.State,
                HealthStatus = container.Status
            });
        }

        return response;
    }

    /// <summary>
    /// Gets the health status for a specific container. If health information is unavailable, the running status is returned.
    /// </summary>
    /// <param name="containerName">The name of the container to inspect.</param>
    /// <returns>Container health details.</returns>
    [HttpGet("health/{containerName}")]
    public async Task<IActionResult> GetContainerHealth(string containerName)
    {
        _logger.LogInformation("Fetching health for container: {ContainerName}", containerName);
        try
        {
            var containerDetails = await _dockerService.InspectContainerAsync(containerName);
            _logger.LogInformation("Successfully fetched details for container: {ContainerName}", containerName);

            // If health information exists, use it; otherwise, fall back to the running state.
            var healthStatus = containerDetails.State.Health?.Status ?? containerDetails.State.Status;

            var result = new
            {
                Id = containerDetails.ID,
                Container = containerName,
                HealthStatus = healthStatus,
                Details = containerDetails.State.Health != null ? (object)containerDetails.State.Health : new { containerDetails.State.Status }
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching details for container: {containerName}", containerName);
            return NotFound(new { ex.Message });
        }
    }
}
