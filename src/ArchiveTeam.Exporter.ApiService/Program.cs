using System.Net;
using ArchiveTeam.Exporter.ApiService.Services;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

builder.Services.AddHttpClient<ProjectService>(client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "ArchiveTeam.Exporter/1.0");
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    AutomaticDecompression = DecompressionMethods.All
});

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpMetrics();
app.MapGet("/metrics", async (ProjectService projectService, HttpContext context, CancellationToken cancellationToken) =>
{
    await projectService.GetProjectGaugesAsync(cancellationToken);
    var registry = Metrics.DefaultRegistry;
    var response = context.Response;
    response.ContentType = PrometheusConstants.TextContentTypeWithVersionAndEncoding;
    await registry.CollectAndExportAsTextAsync(response.Body, cancellationToken);
});

app.MapGet("/", () => "ArchiveTeam Exporter API. Navigate to /metrics for Prometheus metrics.");

app.MapDefaultEndpoints();

app.Run();
