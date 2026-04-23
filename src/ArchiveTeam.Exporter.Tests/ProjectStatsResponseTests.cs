using System.Text.Json;
using ArchiveTeam.Exporter.ApiService.Models;
using Xunit;

namespace ArchiveTeam.Exporter.Tests;

public class ProjectStatsResponseTests
{
    [Fact]
    public void Deserialize_ValidJson_PopulatesAllFields()
    {
        var json = """
        {
            "downloaders": ["user1", "user2"],
            "downloader_bytes": {"user1": 1000000.0, "user2": 2000000.0},
            "downloader_count": {"user1": 50, "user2": 100},
            "domain_bytes": {"data": 5000000.0},
            "total_items_todo": 100,
            "total_items_out": 50,
            "total_items": 200,
            "counts": {
                "done": 150,
                "rcr": 0.75,
                "out": 50,
                "todo": 100
            },
            "total_items_done": 150
        }
        """;

        var result = JsonSerializer.Deserialize<ProjectStatsResponse>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        });

        Assert.NotNull(result);
        Assert.Equal(2, result.Downloaders.Length);
        Assert.Equal(1000000.0, result.DownloaderBytes["user1"]);
        Assert.Equal(2000000.0, result.DownloaderBytes["user2"]);
        Assert.Equal(50, result.DownloaderCount["user1"]);
        Assert.Equal(100, result.DownloaderCount["user2"]);
        Assert.Equal(5000000.0, result.DomainBytes.Data);
        Assert.Equal(100, result.TotalItemsTodo);
        Assert.Equal(50, result.TotalItemsOut);
        Assert.Equal(200, result.TotalItems);
        Assert.Equal(150, result.TotalItemsDone);
        Assert.Equal(150, result.Counts.Done);
        Assert.Equal(0.75, result.Counts.Rcr);
        Assert.Equal(50, result.Counts.Out);
        Assert.Equal(100, result.Counts.Todo);
    }

    [Fact]
    public void Deserialize_MinimalJson_UsesDefaults()
    {
        var json = """
        {
            "downloaders": [],
            "downloader_bytes": {},
            "downloader_count": {},
            "domain_bytes": {"data": 0.0},
            "total_items_todo": 0,
            "total_items_out": 0,
            "total_items": 0,
            "counts": {},
            "total_items_done": 0
        }
        """;

        var result = JsonSerializer.Deserialize<ProjectStatsResponse>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        });

        Assert.NotNull(result);
        Assert.Empty(result.Downloaders);
        Assert.Empty(result.DownloaderBytes);
        Assert.Empty(result.DownloaderCount);
        Assert.Equal(0, result.DomainBytes.Data);
        Assert.Equal(0, result.TotalItemsTodo);
        Assert.Equal(0, result.TotalItemsOut);
        Assert.Equal(0, result.TotalItems);
        Assert.Equal(0, result.TotalItemsDone);
    }

    [Fact]
    public void Deserialize_NullValues()
    {
        var json = """
        {
            "downloaders": null,
            "downloader_bytes": null,
            "downloader_count": null,
            "domain_bytes": null,
            "total_items_todo": 0,
            "total_items_out": 0,
            "total_items": 0,
            "counts": null,
            "total_items_done": 0
        }
        """;

        var result = JsonSerializer.Deserialize<ProjectStatsResponse>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        });

        Assert.NotNull(result);
        Assert.Null(result.Downloaders);
        Assert.Null(result.DownloaderBytes);
        Assert.Null(result.DownloaderCount);
        Assert.Null(result.DomainBytes);
        Assert.Null(result.Counts);
    }
}
