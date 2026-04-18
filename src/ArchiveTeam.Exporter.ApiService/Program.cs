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
    client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
});

builder.Services.AddHostedService<ProjectService>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpMetrics();
app.MapMetrics();

app.MapGet("/", () => "ArchiveTeam Exporter API. Navigate to /metrics for Prometheus metrics.");

app.MapDefaultEndpoints();

app.Run();
