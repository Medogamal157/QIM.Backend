using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using QIM.Application.DTOs.Auth;
using QIM.Application.Interfaces.Auth;
using QIM.Domain.Common.Enums;
using QIM.Presentation.Endpoints;

namespace QIM.Tests.Phase2;

[TestClass]
public class AuthEndpointTests : TestBase
{
    private IAuthService _authService = null!;

    [TestInitialize]
    public async Task Setup()
    {
        _authService = _serviceProvider.GetRequiredService<IAuthService>();

        // Seed required roles
        var roleManager = GetRoleManager();
        foreach (var role in new[] { "Client", "Provider", "Admin", "SuperAdmin", "Moderator", "Support" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // ── 2.20: Access protected endpoint without token → returns 401 ──

    [TestMethod]
    public async Task ChangePassword_WithoutAuth_ReturnsUnauthorized()
    {
        // Controller with no authenticated user (empty HttpContext)
        var controller = new AuthController(_authService)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext() // No claims/identity
            }
        };

        var result = await controller.ChangePassword(new ChangePasswordRequest
        {
            CurrentPassword = "Old@12345",
            NewPassword = "New@12345"
        });

        Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
    }

    // ── 2.21: Access admin endpoint with client role → returns 403 ──

    [TestMethod]
    public async Task AdminLogin_WithClientRole_Returns403()
    {
        // Register a client user
        await _authService.RegisterAsync(new RegisterRequest
        {
            FullName = "Client Only",
            Email = "clientrole@example.com",
            Password = "Test@12345",
            ConfirmPassword = "Test@12345",
            PhoneNumber = "+962799999999",
            UserType = UserType.Client
        });

        var controller = new AuthController(_authService)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        var result = await controller.AdminLogin(new LoginRequest
        {
            Email = "clientrole@example.com",
            Password = "Test@12345"
        });

        var objectResult = result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual(403, objectResult.StatusCode);
    }
}
