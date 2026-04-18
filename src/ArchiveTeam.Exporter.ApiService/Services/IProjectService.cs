using ArchiveTeam.Exporter.ApiService.Models;

namespace ArchiveTeam.Exporter.ApiService.Services;

public interface IProjectService
{
    ArchiveTeamProject[] GetProjects();
}
