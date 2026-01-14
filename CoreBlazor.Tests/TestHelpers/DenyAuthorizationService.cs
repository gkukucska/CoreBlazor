using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CoreBlazor.Tests.TestHelpers;

public class DenyAuthorizationService : IAuthorizationService
{
    public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, IEnumerable<IAuthorizationRequirement> requirements)
    {
        return Task.FromResult(AuthorizationResult.Failed());
    }

    public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, string policyName)
    {
        return Task.FromResult(AuthorizationResult.Failed());
    }

    public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, string policyName)
    {
        return Task.FromResult(AuthorizationResult.Failed());
    }
}
