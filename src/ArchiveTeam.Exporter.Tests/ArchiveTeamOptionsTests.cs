using ArchiveTeam.Exporter.ApiService.Options;
using Xunit;

namespace ArchiveTeam.Exporter.Tests;

public class ArchiveTeamOptionsTests
{
    [Fact]
    public void ValidProjects_AcceptsCommaSeparated()
    {
        var validator = new ValidCommaSeparatedListAttribute();
        var options = new ArchiveTeamOptions { Projects = "proj1,proj2" };

        var result = validator.GetValidationResult(options.Projects, new System.ComponentModel.DataAnnotations.ValidationContext(options));

        Assert.Equal(System.ComponentModel.DataAnnotations.ValidationResult.Success, result);
    }

    [Fact]
    public void EmptyString_FailsValidation()
    {
        var validator = new ValidCommaSeparatedListAttribute();
        var options = new ArchiveTeamOptions { Projects = "" };

        var result = validator.GetValidationResult(options.Projects, new System.ComponentModel.DataAnnotations.ValidationContext(options));

        Assert.NotEqual(System.ComponentModel.DataAnnotations.ValidationResult.Success, result);
    }

    [Fact]
    public void WhitespaceOnly_FailsValidation()
    {
        var validator = new ValidCommaSeparatedListAttribute();
        var options = new ArchiveTeamOptions { Projects = "   " };

        var result = validator.GetValidationResult(options.Projects, new System.ComponentModel.DataAnnotations.ValidationContext(options));

        Assert.NotEqual(System.ComponentModel.DataAnnotations.ValidationResult.Success, result);
    }

    [Fact]
    public void NullValue_FailsValidation()
    {
        var validator = new ValidCommaSeparatedListAttribute();
        var options = new ArchiveTeamOptions { Projects = null! };

        var result = validator.GetValidationResult(options.Projects, new System.ComponentModel.DataAnnotations.ValidationContext(options));

        Assert.NotEqual(System.ComponentModel.DataAnnotations.ValidationResult.Success, result);
    }

    [Fact]
    public void SingleProject_Accepts()
    {
        var validator = new ValidCommaSeparatedListAttribute();
        var options = new ArchiveTeamOptions { Projects = "proj1" };

        var result = validator.GetValidationResult(options.Projects, new System.ComponentModel.DataAnnotations.ValidationContext(options));

        Assert.Equal(System.ComponentModel.DataAnnotations.ValidationResult.Success, result);
    }

    [Fact]
    public void DefaultCacheDurations()
    {
        var options = new ArchiveTeamOptions();

        Assert.Equal(1, options.StatsCacheDurationMinutes);
        Assert.Equal(30, options.ProjectsCacheDurationMinutes);
    }

    [Fact]
    public void CacheDurationProperties()
    {
        var options = new ArchiveTeamOptions
        {
            StatsCacheDurationMinutes = 5,
            ProjectsCacheDurationMinutes = 60
        };

        Assert.Equal(TimeSpan.FromMinutes(5), options.StatsCacheDuration);
        Assert.Equal(TimeSpan.FromMinutes(60), options.ProjectsCacheDuration);
    }

    [Fact]
    public void ProjectsWithTrailingComma_Accepts()
    {
        var validator = new ValidCommaSeparatedListAttribute();
        var options = new ArchiveTeamOptions { Projects = "proj1,proj2," };

        var result = validator.GetValidationResult(options.Projects, new System.ComponentModel.DataAnnotations.ValidationContext(options));

        Assert.Equal(System.ComponentModel.DataAnnotations.ValidationResult.Success, result);
    }

    [Fact]
    public void ProjectsWithWhitespace_Accepts()
    {
        var validator = new ValidCommaSeparatedListAttribute();
        var options = new ArchiveTeamOptions { Projects = "proj1, proj2 , proj3" };

        var result = validator.GetValidationResult(options.Projects, new System.ComponentModel.DataAnnotations.ValidationContext(options));

        Assert.Equal(System.ComponentModel.DataAnnotations.ValidationResult.Success, result);
    }

    [Fact]
    public void Username_DefaultIsEmptyString()
    {
        var options = new ArchiveTeamOptions();

        Assert.Equal(string.Empty, options.Username);
    }
}
