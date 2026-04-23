using Microsoft.AspNetCore.Http;
using Moq;

namespace QIM.Tests.Helpers;

/// <summary>
/// Shared mock helpers used across all test classes.
/// Phase 1 will add: MockHttpContextAccessor, MockUserManager, MockClaimsPrincipal.
/// </summary>
public static class MockHelpers
{
    /// <summary>
    /// Creates a mock IHttpContextAccessor with optional userId claim.
    /// </summary>
    public static IHttpContextAccessor CreateMockHttpContextAccessor(string? userId = null)
    {
        var httpContext = new DefaultHttpContext();

        if (userId is not null)
        {
            var identity = new System.Security.Claims.ClaimsIdentity("TestAuth");
            identity.AddClaim(new System.Security.Claims.Claim(
                System.Security.Claims.ClaimTypes.NameIdentifier, userId));
            httpContext.User = new System.Security.Claims.ClaimsPrincipal(identity);
        }

        var mock = new Mock<IHttpContextAccessor>();
        mock.SetupGet(a => a.HttpContext).Returns(httpContext);
        return mock.Object;
    }

    /// <summary>
    /// Creates a mock IFormFileCollection with the given files.
    /// </summary>
    public static IFormFileCollection CreateFormFileCollection(params IFormFile[] files)
    {
        var collection = new FormFileCollection();
        collection.AddRange(files);
        return collection;
    }
}
