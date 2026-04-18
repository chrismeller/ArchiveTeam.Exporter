using System.Diagnostics.Metrics;
using System.Text.Json;
using ArchiveTeam.Exporter.ApiService.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Prometheus;

namespace ArchiveTeam.Exporter.ApiService.Services;

public class ProjectService : IProjectService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ProjectService> _logger;
    private readonly Gauge _projectInfoGauge;

    public ProjectService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<ProjectService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        _projectInfoGauge = Metrics
            .CreateGauge(
                "archiveteam_projects_info",
                "Information about ArchiveTeam projects",
                new GaugeConfiguration
                {
                    LabelNames = ["name", "title", "description"]
                });
    }

    public async Task<ArchiveTeamProject[]> FetchProjectsAsync(CancellationToken cancellationToken)
    {
        var projectsUrl = _configuration.GetValue<string>("ArchiveTeam:ProjectsUrl", "https://warriorhq.archiveteam.org/projects.json");

        _logger.LogInformation("Fetching projects from {ProjectsUrl}", projectsUrl);

        var response = await _httpClient.GetAsync(projectsUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        var projectsResponse = await response.Content.ReadFromJsonAsync<ArchiveTeamProjectsResponse>(cancellationToken);

        _logger.LogInformation("Successfully loaded {Count} projects", projectsResponse?.Projects?.Length ?? 0);

        return projectsResponse?.Projects ?? [];
    }

    public async Task<ArchiveTeamProject[]> GetProjectGaugesAsync(CancellationToken cancellationToken)
    {
        var projects = await FetchProjectsAsync(cancellationToken);

        foreach (var project in projects)
        {
            var name = SanitizeLabel(project.Name);
            var title = SanitizeLabel(project.Title);
            var description = SanitizeLabel(project.Description);

            _projectInfoGauge
                .WithLabels(name, title, description)
                .Set(1);
        }

        return projects;
    }

    private static string SanitizeLabel(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "";
        }

        return new string(value
            .Where(c => char.IsLetterOrDigit(c) || c == '_')
            .ToArray());
    }
}
