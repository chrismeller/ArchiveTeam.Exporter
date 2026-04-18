var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.ArchiveTeam_Exporter_ApiService>("apiservice")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health");

builder.Build().Run();
