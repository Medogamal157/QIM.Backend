using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using QIM.Domain.Entities.Identity;
using QIM.Application.Extensions;
using QIM.Infrastructure.Extensions;
using QIM.Persistence.Contexts;
using QIM.Persistence.Extensions;
using QIM.Persistence.Seeds;

using QIM.WebApi.Middleware;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/qim-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30)
    .CreateBootstrapLogger();

try
{

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration)
          .WriteTo.Console()
          .WriteTo.File("logs/qim-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30));

// ── CORS ──
var corsSettings = builder.Configuration.GetSection("CorsSettings");
builder.Services.AddCors(options =>
{
    options.AddPolicy("NextJsClient", policy =>
    {
        policy.WithOrigins(corsSettings.GetSection("AllowedOrigins").Get<string[]>() ?? ["http://localhost:3000"])
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ── Persistence ──
builder.Services.AddDbContext<QimDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<QimDbContext>()
    .AddDefaultTokenProviders();

// ── Layer registrations ──
builder.Services.AddApplicationLayer();
builder.Services.AddPersistenceLayer();
builder.Services.AddInfrastructureLayer(builder.Configuration);

// ── Controllers ──
builder.Services.AddControllers();
builder.Services.AddResponseCaching();

// ── Swagger / OpenAPI ──
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "QIM API",
        Version = "v1",
        Description = "QIM Platform – Business Directory & Review API"
    });

    // JWT Bearer auth in Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ── Seed Database ──
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<QimDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await DbSeeder.SeedAsync(context, userManager, roleManager);
}

// ── Swagger middleware (dev only) ──
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "QIM API v1");
        options.RoutePrefix = "swagger";  // available at /swagger
    });
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor
                     | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto,
});
app.UseCors("NextJsClient");
app.UseResponseCaching();
app.UseStaticFiles();           // serve /uploads and other static assets
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// ── Maintenance mode check ──
app.UseMiddleware<MaintenanceModeMiddleware>();

// ── Health check ──
app.MapGet("/api/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.MapControllers();

app.Run();

}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
