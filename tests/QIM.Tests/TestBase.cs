using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using QIM.Application.Interfaces.Auth;
using QIM.Domain.Entities.Identity;
using QIM.Infrastructure.Services.Auth;
using QIM.Persistence.Contexts;
using QIM.Persistence.Repositories;
using System.Text;

namespace QIM.Tests;

/// <summary>
/// Base class for all integration tests.
/// Uses real DI container + SQLite in-memory database.
/// Pattern follows GCC TestBase: resolve real handlers from DI, not manual instantiation.
/// </summary>
public class TestBase
{
    protected WebApplicationBuilder _builder { get; set; }
    protected IServiceProvider _serviceProvider { get; set; }
    protected SqliteConnection _connection { get; set; }

    public TestBase()
    {
        _builder = WebApplication.CreateBuilder();
        _builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);

        // JWT configuration for Phase 2+ auth tests
        _builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "JwtSettings:SecretKey", "SuperSecretKeyForTestingPurposesOnlyMustBeAtLeast256Bits!!" },
            { "JwtSettings:Issuer", "QIM-Test" },
            { "JwtSettings:Audience", "QIM-Test-Client" },
            { "JwtSettings:AccessTokenExpirationMinutes", "60" },
            { "JwtSettings:RefreshTokenExpirationDays", "7" }
        });

        _builder.Services.AddMemoryCache();

        // SQLite in-memory DB + Identity
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        _builder.Services.AddDbContext<QimDbContext>(options =>
            options.UseSqlite(_connection),
            ServiceLifetime.Singleton, ServiceLifetime.Singleton);

        _builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<QimDbContext>()
            .AddDefaultTokenProviders();

        // Phase 2+ auth services
        _builder.Services.AddScoped<IRefreshTokenStore, RefreshTokenStore>();
        _builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        _builder.Services.AddScoped<IAuthService, AuthService>();

        _serviceProvider = _builder.Services.BuildServiceProvider();

        // Create tables
        var context = _serviceProvider.GetRequiredService<QimDbContext>();
        context.GetService<IRelationalDatabaseCreator>()!.CreateTables();
    }

    /// <summary>
    /// Gets the test QimDbContext.
    /// </summary>
    protected QimDbContext GetDbContext() => _serviceProvider.GetRequiredService<QimDbContext>();

    /// <summary>
    /// Creates a mock IFormFile for upload-related tests.
    /// </summary>
    public static IFormFile MockIFormFile(
        string fileName = "testFile.txt",
        string content = "This is a test file.",
        string contentType = "text/plain")
    {
        var contentBytes = Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(contentBytes);

        var formFileMock = new Mock<IFormFile>();
        formFileMock.Setup(f => f.FileName).Returns(fileName);
        formFileMock.Setup(f => f.Length).Returns(stream.Length);
        formFileMock.Setup(f => f.OpenReadStream()).Returns(stream);
        formFileMock.Setup(f => f.ContentType).Returns(contentType);
        formFileMock.Setup(f => f.ContentDisposition)
                    .Returns($"form-data; name=\"file\"; filename=\"{fileName}\"");

        return formFileMock.Object;
    }

    /// <summary>
    /// Creates a mock IFormFile specifically for image uploads.
    /// </summary>
    public static IFormFile MockImageFormFile(
        string fileName = "test-image.jpg",
        string contentType = "image/jpeg")
    {
        // 1x1 pixel JPEG
        var imageBytes = new byte[]
        {
            0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01,
            0x01, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0xFF, 0xD9
        };
        var stream = new MemoryStream(imageBytes);

        var formFileMock = new Mock<IFormFile>();
        formFileMock.Setup(f => f.FileName).Returns(fileName);
        formFileMock.Setup(f => f.Length).Returns(stream.Length);
        formFileMock.Setup(f => f.OpenReadStream()).Returns(stream);
        formFileMock.Setup(f => f.ContentType).Returns(contentType);
        formFileMock.Setup(f => f.ContentDisposition)
                    .Returns($"form-data; name=\"file\"; filename=\"{fileName}\"");

        return formFileMock.Object;
    }

    /// <summary>
    /// Gets the UserManager from DI.
    /// </summary>
    protected UserManager<ApplicationUser> GetUserManager() =>
        _serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    /// <summary>
    /// Gets the RoleManager from DI.
    /// </summary>
    protected RoleManager<IdentityRole> GetRoleManager() =>
        _serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
}
