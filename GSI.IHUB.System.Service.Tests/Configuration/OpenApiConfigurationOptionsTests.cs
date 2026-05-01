using GSI.IHUB.System.Service.Configuration;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;

namespace GSI.IHUB.System.Service.Tests.Configuration;

public class OpenApiConfigurationOptionsTests
{
    private readonly OpenApiConfigurationOptions _sut = new();

    // ──────────────────────────────────────────────────────────
    // Implements the expected interface
    // ──────────────────────────────────────────────────────────

    [Fact]
    public void Options_ImplementsIOpenApiConfigurationOptions()
    {
        Assert.IsAssignableFrom<IOpenApiConfigurationOptions>(_sut);
    }

    // ──────────────────────────────────────────────────────────
    // Info — title, version, description, contact
    // ──────────────────────────────────────────────────────────

    [Fact]
    public void Info_IsNotNull()
    {
        Assert.NotNull(_sut.Info);
    }

    [Fact]
    public void Info_Title_IsExpected()
    {
        Assert.Equal("GSI IHUB System API", _sut.Info.Title);
    }

    [Fact]
    public void Info_Version_IsExpected()
    {
        Assert.Equal("v1.0.0", _sut.Info.Version);
    }

    [Fact]
    public void Info_Description_IsNotEmpty()
    {
        Assert.False(string.IsNullOrWhiteSpace(_sut.Info.Description));
    }

    [Fact]
    public void Info_Contact_IsNotNull()
    {
        Assert.NotNull(_sut.Info.Contact);
    }

    [Fact]
    public void Info_Contact_Name_IsNotEmpty()
    {
        Assert.False(string.IsNullOrWhiteSpace(_sut.Info.Contact?.Name));
    }

    // ──────────────────────────────────────────────────────────
    // OpenAPI version
    // ──────────────────────────────────────────────────────────

    [Fact]
    public void OpenApiVersion_IsV3()
    {
        Assert.Equal(OpenApiVersionType.V3, _sut.OpenApiVersion);
    }
}
