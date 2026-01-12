using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace CoreBlazor.Tests.TestHelpers;

/// <summary>
/// Helper for creating test authentication states
/// </summary>
public static class AuthenticationHelper
{
    /// <summary>
    /// Creates an authenticated state with the specified role
    /// </summary>
    public static Task<AuthenticationState> CreateAuthenticationState(string role = "Admin")
    {
        var claims = new[] { new Claim(ClaimTypes.Role, role) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);
        return Task.FromResult(new AuthenticationState(user));
    }

    /// <summary>
    /// Creates an unauthenticated state
    /// </summary>
    public static Task<AuthenticationState> CreateUnauthenticatedState()
    {
        var identity = new ClaimsIdentity();
        var user = new ClaimsPrincipal(identity);
        return Task.FromResult(new AuthenticationState(user));
    }
}
