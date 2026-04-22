using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using QIM.Application.DTOs.Auth;
using QIM.Application.Interfaces.Auth;
using QIM.Domain.Common.Enums;
using QIM.Domain.Entities.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace QIM.Tests.Phase2;

[TestClass]
public class AuthServiceTests : TestBase
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

    // ── 2.12: Register client → returns success + JWT ──

    [TestMethod]
    public async Task RegisterClient_ReturnsSuccessAndJwt()
    {
        var request = new RegisterRequest
        {
            FullName = "Test Client",
            Email = "client@example.com",
            Password = "Test@12345",
            ConfirmPassword = "Test@12345",
            PhoneNumber = "+962799999999",
            UserType = UserType.Client
        };

        var result = await _authService.RegisterAsync(request);

        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Data);
        Assert.IsFalse(string.IsNullOrEmpty(result.Data.AccessToken));
        Assert.IsFalse(string.IsNullOrEmpty(result.Data.RefreshToken));
        Assert.AreEqual("client@example.com", result.Data.User.Email);
        Assert.AreEqual("Test Client", result.Data.User.FullName);
        Assert.IsTrue(result.Data.User.Roles.Contains("Client"));
    }

    // ── 2.13: Register with duplicate email → returns error ──

    [TestMethod]
    public async Task RegisterDuplicateEmail_ReturnsError()
    {
        var request = new RegisterRequest
        {
            FullName = "First User",
            Email = "dupe@example.com",
            Password = "Test@12345",
            ConfirmPassword = "Test@12345",
            PhoneNumber = "+962799999999",
            UserType = UserType.Client
        };

        var first = await _authService.RegisterAsync(request);
        Assert.IsTrue(first.IsSuccess, "First registration should succeed.");

        var second = await _authService.RegisterAsync(request);

        Assert.IsFalse(second.IsSuccess);
        Assert.IsTrue(second.Errors.Any(e => e.Contains("already exists")));
    }

    // ── 2.14: Login with valid credentials → returns JWT + refresh token ──

    [TestMethod]
    public async Task LoginValid_ReturnsJwtAndRefreshToken()
    {
        await _authService.RegisterAsync(new RegisterRequest
        {
            FullName = "Login User",
            Email = "login@example.com",
            Password = "Test@12345",
            ConfirmPassword = "Test@12345",
            PhoneNumber = "+962799999999",
            UserType = UserType.Client
        });

        var result = await _authService.LoginAsync(new LoginRequest
        {
            Email = "login@example.com",
            Password = "Test@12345"
        });

        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Data);
        Assert.IsFalse(string.IsNullOrEmpty(result.Data.AccessToken));
        Assert.IsFalse(string.IsNullOrEmpty(result.Data.RefreshToken));
        Assert.AreEqual("login@example.com", result.Data.User.Email);
    }

    // ── 2.15: Login with wrong password → returns error ──

    [TestMethod]
    public async Task LoginWrongPassword_ReturnsError()
    {
        await _authService.RegisterAsync(new RegisterRequest
        {
            FullName = "Wrong Pass User",
            Email = "wrongpass@example.com",
            Password = "Test@12345",
            ConfirmPassword = "Test@12345",
            PhoneNumber = "+962799999999",
            UserType = UserType.Client
        });

        var result = await _authService.LoginAsync(new LoginRequest
        {
            Email = "wrongpass@example.com",
            Password = "WrongPassword123!"
        });

        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.Errors.Any(e => e.Contains("Invalid email or password")));
    }

    // ── 2.16: Admin login with non-admin user → returns Access Denied ──

    [TestMethod]
    public async Task AdminLoginNonAdmin_ReturnsAccessDenied()
    {
        await _authService.RegisterAsync(new RegisterRequest
        {
            FullName = "Client User",
            Email = "clientonly@example.com",
            Password = "Test@12345",
            ConfirmPassword = "Test@12345",
            PhoneNumber = "+962799999999",
            UserType = UserType.Client
        });

        var result = await _authService.AdminLoginAsync(new LoginRequest
        {
            Email = "clientonly@example.com",
            Password = "Test@12345"
        });

        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.Errors.Any(e => e.Contains("Access denied")));
    }

    // ── 2.17: Admin login with admin user → returns JWT ──

    [TestMethod]
    public async Task AdminLoginWithAdmin_ReturnsJwt()
    {
        // Create admin user directly via UserManager
        var userManager = GetUserManager();
        var adminUser = new ApplicationUser
        {
            UserName = "admin@example.com",
            Email = "admin@example.com",
            FullName = "Admin User",
            UserType = UserType.Admin,
            IsActive = true
        };
        await userManager.CreateAsync(adminUser, "Admin@12345");
        await userManager.AddToRoleAsync(adminUser, "Admin");

        var result = await _authService.AdminLoginAsync(new LoginRequest
        {
            Email = "admin@example.com",
            Password = "Admin@12345"
        });

        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Data);
        Assert.IsFalse(string.IsNullOrEmpty(result.Data.AccessToken));
        Assert.IsFalse(string.IsNullOrEmpty(result.Data.RefreshToken));
        Assert.IsTrue(result.Data.User.Roles.Contains("Admin"));
    }

    // ── 2.18: Refresh token → returns new JWT pair ──

    [TestMethod]
    public async Task RefreshToken_ReturnsNewJwtPair()
    {
        var registerResult = await _authService.RegisterAsync(new RegisterRequest
        {
            FullName = "Refresh User",
            Email = "refresh@example.com",
            Password = "Test@12345",
            ConfirmPassword = "Test@12345",
            PhoneNumber = "+962799999999",
            UserType = UserType.Client
        });

        Assert.IsTrue(registerResult.IsSuccess);

        var refreshResult = await _authService.RefreshTokenAsync(new RefreshTokenRequest
        {
            RefreshToken = registerResult.Data!.RefreshToken
        });

        Assert.IsTrue(refreshResult.IsSuccess);
        Assert.IsNotNull(refreshResult.Data);
        Assert.IsFalse(string.IsNullOrEmpty(refreshResult.Data.AccessToken));
        Assert.IsFalse(string.IsNullOrEmpty(refreshResult.Data.RefreshToken));
        // New refresh token should differ from old (rotation)
        Assert.AreNotEqual(registerResult.Data.RefreshToken, refreshResult.Data.RefreshToken);
    }

    // ── 2.19: Refresh with revoked token → returns error ──

    [TestMethod]
    public async Task RefreshRevokedToken_ReturnsError()
    {
        var registerResult = await _authService.RegisterAsync(new RegisterRequest
        {
            FullName = "Revoke User",
            Email = "revoke@example.com",
            Password = "Test@12345",
            ConfirmPassword = "Test@12345",
            PhoneNumber = "+962799999999",
            UserType = UserType.Client
        });

        Assert.IsTrue(registerResult.IsSuccess);

        // First refresh rotates the token (revokes original)
        var firstRefresh = await _authService.RefreshTokenAsync(new RefreshTokenRequest
        {
            RefreshToken = registerResult.Data!.RefreshToken
        });
        Assert.IsTrue(firstRefresh.IsSuccess);

        // Second refresh with the original (now revoked) token should fail
        var secondRefresh = await _authService.RefreshTokenAsync(new RefreshTokenRequest
        {
            RefreshToken = registerResult.Data.RefreshToken
        });

        Assert.IsFalse(secondRefresh.IsSuccess);
        Assert.IsTrue(secondRefresh.Errors.Any(e => e.Contains("revoked")));
    }

    // ── Additional coverage: Registration edge cases ──

    [TestMethod]
    public async Task RegisterProvider_AssignsProviderRole()
    {
        var result = await _authService.RegisterAsync(new RegisterRequest
        {
            FullName = "Provider User",
            Email = "provider@example.com",
            Password = "Test@12345",
            ConfirmPassword = "Test@12345",
            PhoneNumber = "+962799999999",
            UserType = UserType.Provider
        });

        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(result.Data!.User.Roles.Contains("Provider"));
        Assert.AreEqual("Provider", result.Data.User.UserType);
    }

    [TestMethod]
    public async Task Register_PasswordMismatch_ReturnsError()
    {
        var result = await _authService.RegisterAsync(new RegisterRequest
        {
            FullName = "Mismatch User",
            Email = "mismatch@example.com",
            Password = "Test@12345",
            ConfirmPassword = "Different@12345",
            PhoneNumber = "+962799999999",
            UserType = UserType.Client
        });

        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.Errors.Any(e => e.Contains("Passwords do not match")));
    }

    // ── Additional coverage: Login edge cases ──

    [TestMethod]
    public async Task Login_NonExistentEmail_ReturnsError()
    {
        var result = await _authService.LoginAsync(new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "Test@12345"
        });

        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.Errors.Any(e => e.Contains("Invalid email or password")));
    }

    [TestMethod]
    public async Task Login_DeactivatedUser_ReturnsError()
    {
        var userManager = GetUserManager();
        var user = new ApplicationUser
        {
            UserName = "deactivated@example.com",
            Email = "deactivated@example.com",
            FullName = "Deactivated User",
            UserType = UserType.Client,
            IsActive = false
        };
        await userManager.CreateAsync(user, "Test@12345");
        await userManager.AddToRoleAsync(user, "Client");

        var result = await _authService.LoginAsync(new LoginRequest
        {
            Email = "deactivated@example.com",
            Password = "Test@12345"
        });

        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.Errors.Any(e => e.Contains("deactivated")));
    }

    [TestMethod]
    public async Task AdminLogin_DeactivatedAdmin_ReturnsError()
    {
        var userManager = GetUserManager();
        var user = new ApplicationUser
        {
            UserName = "deactivatedadmin@example.com",
            Email = "deactivatedadmin@example.com",
            FullName = "Deactivated Admin",
            UserType = UserType.Admin,
            IsActive = false
        };
        await userManager.CreateAsync(user, "Admin@12345");
        await userManager.AddToRoleAsync(user, "Admin");

        var result = await _authService.AdminLoginAsync(new LoginRequest
        {
            Email = "deactivatedadmin@example.com",
            Password = "Admin@12345"
        });

        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.Errors.Any(e => e.Contains("deactivated")));
    }

    // ── Additional coverage: Logout ──

    [TestMethod]
    public async Task Logout_RevokesRefreshToken()
    {
        var registerResult = await _authService.RegisterAsync(new RegisterRequest
        {
            FullName = "Logout User",
            Email = "logout@example.com",
            Password = "Test@12345",
            ConfirmPassword = "Test@12345",
            PhoneNumber = "+962799999999",
            UserType = UserType.Client
        });
        Assert.IsTrue(registerResult.IsSuccess);

        var logoutResult = await _authService.LogoutAsync(registerResult.Data!.RefreshToken);
        Assert.IsTrue(logoutResult.IsSuccess);

        // Refresh with the now-revoked token should fail
        var refreshResult = await _authService.RefreshTokenAsync(new RefreshTokenRequest
        {
            RefreshToken = registerResult.Data.RefreshToken
        });
        Assert.IsFalse(refreshResult.IsSuccess);
        Assert.IsTrue(refreshResult.Errors.Any(e => e.Contains("revoked")));
    }

    // ── Additional coverage: Change password ──

    [TestMethod]
    public async Task ChangePassword_ValidRequest_Succeeds()
    {
        var registerResult = await _authService.RegisterAsync(new RegisterRequest
        {
            FullName = "Change Pass User",
            Email = "changepass@example.com",
            Password = "Test@12345",
            ConfirmPassword = "Test@12345",
            PhoneNumber = "+962799999999",
            UserType = UserType.Client
        });
        Assert.IsTrue(registerResult.IsSuccess);

        var changeResult = await _authService.ChangePasswordAsync(
            registerResult.Data!.User.Id,
            new ChangePasswordRequest
            {
                CurrentPassword = "Test@12345",
                NewPassword = "NewPass@12345"
            });

        Assert.IsTrue(changeResult.IsSuccess);

        // Can login with new password
        var loginResult = await _authService.LoginAsync(new LoginRequest
        {
            Email = "changepass@example.com",
            Password = "NewPass@12345"
        });
        Assert.IsTrue(loginResult.IsSuccess);

        // Old password no longer works
        var oldPassResult = await _authService.LoginAsync(new LoginRequest
        {
            Email = "changepass@example.com",
            Password = "Test@12345"
        });
        Assert.IsFalse(oldPassResult.IsSuccess);
    }

    [TestMethod]
    public async Task ChangePassword_WrongCurrentPassword_ReturnsError()
    {
        var registerResult = await _authService.RegisterAsync(new RegisterRequest
        {
            FullName = "Wrong Current User",
            Email = "wrongcurrent@example.com",
            Password = "Test@12345",
            ConfirmPassword = "Test@12345",
            PhoneNumber = "+962799999999",
            UserType = UserType.Client
        });
        Assert.IsTrue(registerResult.IsSuccess);

        var result = await _authService.ChangePasswordAsync(
            registerResult.Data!.User.Id,
            new ChangePasswordRequest
            {
                CurrentPassword = "WrongPassword@1",
                NewPassword = "NewPass@12345"
            });

        Assert.IsFalse(result.IsSuccess);
    }

    [TestMethod]
    public async Task ChangePassword_RevokesAllRefreshTokens()
    {
        var registerResult = await _authService.RegisterAsync(new RegisterRequest
        {
            FullName = "Revoke All User",
            Email = "revokeall@example.com",
            Password = "Test@12345",
            ConfirmPassword = "Test@12345",
            PhoneNumber = "+962799999999",
            UserType = UserType.Client
        });
        Assert.IsTrue(registerResult.IsSuccess);

        await _authService.ChangePasswordAsync(
            registerResult.Data!.User.Id,
            new ChangePasswordRequest
            {
                CurrentPassword = "Test@12345",
                NewPassword = "NewPass@12345"
            });

        // Old refresh token should be revoked
        var refreshResult = await _authService.RefreshTokenAsync(new RefreshTokenRequest
        {
            RefreshToken = registerResult.Data.RefreshToken
        });
        Assert.IsFalse(refreshResult.IsSuccess);
    }

    // ── Additional coverage: Forgot / Reset password ──

    [TestMethod]
    public async Task ForgotPassword_ExistingEmail_ReturnsSuccess()
    {
        await _authService.RegisterAsync(new RegisterRequest
        {
            FullName = "Forgot User",
            Email = "forgot@example.com",
            Password = "Test@12345",
            ConfirmPassword = "Test@12345",
            PhoneNumber = "+962799999999",
            UserType = UserType.Client
        });

        var result = await _authService.ForgotPasswordAsync(new ForgotPasswordRequest
        {
            Email = "forgot@example.com"
        });

        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(LastEmailHtml);
        Assert.IsTrue(LastEmailHtml!.Contains("token="));
    }

    [TestMethod]
    public async Task ForgotPassword_NonExistentEmail_StillReturnsSuccess()
    {
        // Security: don't reveal whether email exists
        var result = await _authService.ForgotPasswordAsync(new ForgotPasswordRequest
        {
            Email = "nobody@example.com"
        });

        Assert.IsTrue(result.IsSuccess);
    }

    [TestMethod]
    public async Task ResetPassword_ValidToken_Succeeds()
    {
        await _authService.RegisterAsync(new RegisterRequest
        {
            FullName = "Reset User",
            Email = "reset@example.com",
            Password = "Test@12345",
            ConfirmPassword = "Test@12345",
            PhoneNumber = "+962799999999",
            UserType = UserType.Client
        });

        var forgotResult = await _authService.ForgotPasswordAsync(new ForgotPasswordRequest
        {
            Email = "reset@example.com"
        });

        // Extract token from captured reset email URL
        Assert.IsNotNull(LastEmailHtml);
        var tokenMatch = System.Text.RegularExpressions.Regex.Match(LastEmailHtml!, "token=([^\"&]+)");
        Assert.IsTrue(tokenMatch.Success, "Reset email did not contain token");
        var token = System.Net.WebUtility.UrlDecode(tokenMatch.Groups[1].Value);

        var resetResult = await _authService.ResetPasswordAsync(new ResetPasswordRequest
        {
            Email = "reset@example.com",
            Token = token,
            NewPassword = "ResetPass@12345"
        });

        Assert.IsTrue(resetResult.IsSuccess);

        // Can login with new password
        var loginResult = await _authService.LoginAsync(new LoginRequest
        {
            Email = "reset@example.com",
            Password = "ResetPass@12345"
        });
        Assert.IsTrue(loginResult.IsSuccess);
    }

    [TestMethod]
    public async Task ResetPassword_InvalidToken_ReturnsError()
    {
        await _authService.RegisterAsync(new RegisterRequest
        {
            FullName = "Invalid Token User",
            Email = "invalidtoken@example.com",
            Password = "Test@12345",
            ConfirmPassword = "Test@12345",
            PhoneNumber = "+962799999999",
            UserType = UserType.Client
        });

        var result = await _authService.ResetPasswordAsync(new ResetPasswordRequest
        {
            Email = "invalidtoken@example.com",
            Token = "totally-invalid-token",
            NewPassword = "NewPass@12345"
        });

        Assert.IsFalse(result.IsSuccess);
    }

    // ── Additional coverage: Refresh token edge cases ──

    [TestMethod]
    public async Task RefreshToken_NonExistentToken_ReturnsError()
    {
        var result = await _authService.RefreshTokenAsync(new RefreshTokenRequest
        {
            RefreshToken = "this-token-does-not-exist"
        });

        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.Errors.Any(e => e.Contains("Invalid refresh token")));
    }

    // ── Additional coverage: JWT claims verification ──

    [TestMethod]
    public async Task Jwt_ContainsCorrectClaims()
    {
        var registerResult = await _authService.RegisterAsync(new RegisterRequest
        {
            FullName = "Claims User",
            Email = "claims@example.com",
            Password = "Test@12345",
            ConfirmPassword = "Test@12345",
            PhoneNumber = "+962799999999",
            UserType = UserType.Client
        });

        Assert.IsTrue(registerResult.IsSuccess);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(registerResult.Data!.AccessToken);

        Assert.AreEqual("claims@example.com", jwt.Claims.First(c => c.Type == ClaimTypes.Email || c.Type == "email").Value);
        Assert.AreEqual("Claims User", jwt.Claims.First(c => c.Type == "fullName").Value);
        Assert.AreEqual("Client", jwt.Claims.First(c => c.Type == "userType").Value);
        Assert.IsTrue(jwt.Claims.Any(c =>
            (c.Type == ClaimTypes.Role || c.Type == "role") && c.Value == "Client"));
        Assert.IsNotNull(jwt.Claims.First(c =>
            c.Type == ClaimTypes.NameIdentifier || c.Type == "nameid").Value);
    }

    [TestMethod]
    public async Task Jwt_ExpiresAt_IsInFuture()
    {
        var registerResult = await _authService.RegisterAsync(new RegisterRequest
        {
            FullName = "Expiry User",
            Email = "expiry@example.com",
            Password = "Test@12345",
            ConfirmPassword = "Test@12345",
            PhoneNumber = "+962799999999",
            UserType = UserType.Client
        });

        Assert.IsTrue(registerResult.IsSuccess);
        Assert.IsTrue(registerResult.Data!.ExpiresAt > DateTime.UtcNow);
    }

    // ── Additional coverage: AuthResponse shape ──

    [TestMethod]
    public async Task Register_AuthResponse_HasCorrectUserDto()
    {
        var result = await _authService.RegisterAsync(new RegisterRequest
        {
            FullName = "Dto User",
            Email = "dto@example.com",
            Password = "Test@12345",
            ConfirmPassword = "Test@12345",
            PhoneNumber = "+962791234567",
            UserType = UserType.Client
        });

        Assert.IsTrue(result.IsSuccess);
        var user = result.Data!.User;
        Assert.AreEqual("Dto User", user.FullName);
        Assert.AreEqual("dto@example.com", user.Email);
        Assert.AreEqual("+962791234567", user.PhoneNumber);
        Assert.AreEqual("Client", user.UserType);
        Assert.IsFalse(user.IsVerified);
        Assert.IsNull(user.ProfileImageUrl);
        Assert.IsFalse(string.IsNullOrEmpty(user.Id));
    }
}
