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
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public void SignOut()
        {
            CurrentUser = null;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
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
        public static CustomUser Editor { get; } = new() { Name = nameof(Editor) };
        public static CustomUser Reader { get; } = new() { Name = nameof(Reader) };
        private CustomUser(): base(new ClaimsIdentity() { }) {
        }
    }
}
