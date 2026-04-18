using ArchiveTeam.Exporter.ApiService.Models;

namespace ArchiveTeam.Exporter.ApiService.Services;

public interface IProjectService
{
    Task<ArchiveTeamProject[]> FetchProjectsAsync(CancellationToken cancellationToken);
    Task<ArchiveTeamProject[]> GetProjectGaugesAsync(CancellationToken cancellationToken);
}
