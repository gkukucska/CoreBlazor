using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CoreBlazor.Tests.TestHelpers;

public class ConfigurableAuthorizationService : IAuthorizationService
{
    private readonly HashSet<string> _failingPolicies;

    public ConfigurableAuthorizationService(IEnumerable<string>? failingPolicies = null)
    {
        _failingPolicies = failingPolicies is null ? new HashSet<string>() : new HashSet<string>(failingPolicies);
    }

    public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, IEnumerable<IAuthorizationRequirement> requirements)
    {
        // Default to success for requirement-based checks in tests
        return Task.FromResult(AuthorizationResult.Success());
    }

    public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, string policyName)
    {
        if (policyName is not null && _failingPolicies.Contains(policyName))
            return Task.FromResult(AuthorizationResult.Failed());
        return Task.FromResult(AuthorizationResult.Success());
    }

    public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, string policyName)
    {
        if (policyName is not null && _failingPolicies.Contains(policyName))
            return Task.FromResult(AuthorizationResult.Failed());
        return Task.FromResult(AuthorizationResult.Success());
    }
}
