using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO.Compression;
using System.Text.Json;
using ArchiveTeam.Exporter.ApiService.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prometheus;

namespace ArchiveTeam.Exporter.ApiService.Services;

public class ProjectService : BackgroundService, IProjectService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ProjectService> _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    
    private ArchiveTeamProject[] _projects = [];
    private readonly Gauge _projectInfoGauge;
    private readonly HashSet<(string name, string title, string description)> _previousLabels = new();

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

    public ArchiveTeamProject[] GetProjects()
    {
        _semaphore.Wait();
        try
        {
            return _projects.ToArray();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var refreshIntervalMinutes = _configuration.GetValue<int>("ArchiveTeam:RefreshIntervalMinutes", 5);
        var refreshInterval = TimeSpan.FromMinutes(refreshIntervalMinutes);
        
        _logger.LogInformation("ProjectService starting with refresh interval: {RefreshInterval}", refreshInterval);
        
        await FetchProjectsAsync(stoppingToken);
        
        using var timer = new PeriodicTimer(refreshInterval);
        
        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            await FetchProjectsAsync(stoppingToken);
        }
    }

    private async Task FetchProjectsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var projectsUrl = _configuration.GetValue<string>("ArchiveTeam:ProjectsUrl", "https://warriorhq.archiveteam.org/projects.json");
            
            _logger.LogInformation("Fetching projects from {ProjectsUrl}", projectsUrl);
            
            var response = await _httpClient.GetAsync(projectsUrl, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            string json;
            
            if (response.Content.Headers.ContentEncoding.Contains("gzip"))
            {
                using var gzipStream = new GZipStream(new MemoryStream(bytes), CompressionMode.Decompress);
                using var reader = new StreamReader(gzipStream);
                json = await reader.ReadToEndAsync(cancellationToken);
            }
            else
            {
                json = System.Text.Encoding.UTF8.GetString(bytes);
            }
            var projectsResponse = JsonSerializer.Deserialize<ArchiveTeamProjectsResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            if (projectsResponse?.Projects != null)
            {
                await _semaphore.WaitAsync(cancellationToken);
                try
                {
                    var currentLabels = new HashSet<(string name, string title, string description)>();
                    
                    foreach (var project in projectsResponse.Projects)
                    {
                        var name = SanitizeLabel(project.Name);
                        var title = SanitizeLabel(project.Title);
                        var description = SanitizeLabel(project.Description);
                        var labelTuple = (name, title, description);
                        
                        currentLabels.Add(labelTuple);
                        
                        _projectInfoGauge
                            .WithLabels(name, title, description)
                            .Set(1);
                    }
                    
                    var labelsToRemove = _previousLabels.Except(currentLabels).ToList();
                    foreach (var label in labelsToRemove)
                    {
                        _projectInfoGauge
                            .WithLabels(label.name, label.title, label.description)
                            .Unpublish();
                    }
                    
                    _previousLabels.Clear();
                    foreach (var label in currentLabels)
                    {
                        _previousLabels.Add(label);
                    }
                    
                    _projects = projectsResponse.Projects;
                    _logger.LogInformation("Successfully loaded {Count} projects", _projects.Length);
                }
                finally
                {
                    _semaphore.Release();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch projects");
        }
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

    public override void Dispose()
    {
        _semaphore.Dispose();
        base.Dispose();
    }
}
