namespace ArchiveTeam.Exporter.ApiService.Models;

public class ArchiveTeamProject
{
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Repository { get; set; }
    public string? Leaderboard { get; set; }
    public string? Logo { get; set; }
    public double[] LatLng { get; set; } = [];
    public string? MarkerHtml { get; set; }
}
