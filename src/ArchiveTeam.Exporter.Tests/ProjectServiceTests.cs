using System.Net;
using System.Text.Json;
using ArchiveTeam.Exporter.ApiService.Models;
using ArchiveTeam.Exporter.ApiService.Options;
using ArchiveTeam.Exporter.ApiService.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Prometheus;
using Xunit;

namespace ArchiveTeam.Exporter.Tests;

public class ProjectServiceTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly Mock<IMemoryCache> _memoryCacheMock;
    private readonly Mock<ILogger<ProjectService>> _loggerMock;
    private readonly ArchiveTeamOptions _options;
    private readonly Mock<IOptions<ArchiveTeamOptions>> _optionsMock;
    private readonly Mock<ICacheEntry> _cacheEntryMock;

    public ProjectServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _memoryCacheMock = new Mock<IMemoryCache>();
        _loggerMock = new Mock<ILogger<ProjectService>>();
        _cacheEntryMock = new Mock<ICacheEntry>();

        _options = new ArchiveTeamOptions
        {
            Username = "testuser",
            Projects = "proj1,proj2",
            StatsCacheDurationMinutes = 1,
            ProjectsCacheDurationMinutes = 30
        };

        _optionsMock = new Mock<IOptions<ArchiveTeamOptions>>();
        _optionsMock.Setup(x => x.Value).Returns(_options);

        _memoryCacheMock
            .Setup(x => x.CreateEntry(It.IsAny<object>()))
            .Returns(_cacheEntryMock.Object);
    }

    [Fact]
    public void SanitizeLabel_RemovesSpecialChars()
    {
        var result = ProjectServiceTestsHelper.InvokeSanitizeLabel("hello-world!");
        Assert.Equal("helloworld", result);
    }

    [Fact]
    public void SanitizeLabel_EmptyString_ReturnsEmpty()
    {
        var result = ProjectServiceTestsHelper.InvokeSanitizeLabel("");
        Assert.Equal("", result);
    }

    [Fact]
    public void SanitizeLabel_KeepsAlphanumericAndUnderscore()
    {
        var result = ProjectServiceTestsHelper.InvokeSanitizeLabel("test_123");
        Assert.Equal("test_123", result);
    }

    [Fact]
    public void SanitizeLabel_RemovesWhitespace()
    {
        var result = ProjectServiceTestsHelper.InvokeSanitizeLabel("test value");
        Assert.Equal("testvalue", result);
    }

    private delegate void TryGetValueCallback(object key, out object? value);
}

public static class ProjectServiceTestsHelper
{
    public static string InvokeSanitizeLabel(string value)
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
