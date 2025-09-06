using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace CoreBlazorDemo.Authentication
{
    public class DemoAuthenticationStateProvider : AuthenticationStateProvider
    {
        public CustomUser? CurrentUser { get; private set; }
        public bool IsSignedIn => CurrentUser is not null;
        public void SignIn(CustomUser? user)
        {
            CurrentUser = user;
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(CurrentUser)));
        }

        public void SignOut()
        {
            CurrentUser = null;
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal())));
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return Task.FromResult(new AuthenticationState(CurrentUser ?? new ClaimsPrincipal()));
        }
    }

    public class CustomUser : ClaimsPrincipal
    {
        public required string Name { get; set; }
        public static CustomUser Admin { get; } = new() { Name = nameof(Admin) };
        public static CustomUser Engineer { get; } = new() { Name = nameof(Engineer) };
        public static CustomUser Operator { get; } = new() { Name = nameof(Operator) };
        private CustomUser(): base(new ClaimsIdentity() { }) {
        }
    }
}
