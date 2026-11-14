using System.Security.Claims;
using CRM_System.Core.Common;
using CRM_System.Core.Students.DTOs;
using CRM_System.Core.Students.Interfaces;
using CRM_System.Core.Students.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace CRM_System.Web.Auth;

public class CrmAuthStateProvider : AuthenticationStateProvider
{
    private const string StorageKey = "crm_authenticated_user_v1";
    private static readonly AuthenticationState AnonymousState = new(new ClaimsPrincipal(new ClaimsIdentity()));

    private readonly ProtectedLocalStorage _localStorage;
    private readonly IServiceScopeFactory _scopeFactory;
    private AuthenticatedUserDto? _currentUser;
    private bool _initializedFromStorage;

    public CrmAuthStateProvider(
        ProtectedLocalStorage localStorage,
        IServiceScopeFactory scopeFactory)
    {
        _localStorage = localStorage;
        _scopeFactory = scopeFactory;
    }

    public AuthenticatedUserDto? CurrentUser => _currentUser;

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (_currentUser is null || !_currentUser.IsActive)
        {
            return Task.FromResult(AnonymousState);
        }

        return Task.FromResult(BuildAuthenticationState(_currentUser));
    }

    public async Task EnsureInitializedAsync()
    {
        if (_initializedFromStorage)
        {
            return;
        }

        try
        {
            var stored = await _localStorage.GetAsync<int>(StorageKey);
            _initializedFromStorage = true;

            if (stored.Success && stored.Value > 0)
            {
                using var scope = _scopeFactory.CreateScope();
                var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
                var freshUser = await authService.GetUserByIdAsync(stored.Value);
                if (freshUser is not null && freshUser.IsActive)
                {
                    _currentUser = freshUser;
                    NotifyAuthenticationStateChanged(Task.FromResult(BuildAuthenticationState(_currentUser)));
                }
                else
                {
                    await _localStorage.DeleteAsync(StorageKey);
                }
            }
        }
        catch
        {
            // During static prerendering JS interop is not yet available; safe to ignore until OnAfterRenderAsync.
        }
    }

    public async Task SignInAsync(AuthenticatedUserDto user)
    {
        _currentUser = user;
        _initializedFromStorage = true;

        try
        {
            await _localStorage.SetAsync(StorageKey, user.UserId);
        }
        catch
        {
            // Ignore storage errors if called outside interactive circuit
        }

        NotifyAuthenticationStateChanged(Task.FromResult(BuildAuthenticationState(user)));
    }

    public async Task RefreshCurrentUserAsync()
    {
        if (_currentUser is null)
        {
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var refreshed = await authService.GetUserByIdAsync(_currentUser.UserId);
        if (refreshed is not null && refreshed.IsActive)
        {
            _currentUser = refreshed;
            NotifyAuthenticationStateChanged(Task.FromResult(BuildAuthenticationState(_currentUser)));
        }
        else
        {
            await SignOutAsync();
        }
    }

    public async Task SignOutAsync()
    {
        _currentUser = null;
        _initializedFromStorage = true;

        try
        {
            await _localStorage.DeleteAsync(StorageKey);
        }
        catch
        {
            // Ignore storage errors if called outside interactive circuit
        }

        NotifyAuthenticationStateChanged(Task.FromResult(AnonymousState));
    }

    public AccessContext GetAccessContext()
    {
        if (_currentUser is null || !_currentUser.IsActive)
        {
            return AccessContext.Anonymous;
        }

        return new AccessContext(
            _currentUser.UserId,
            _currentUser.StudentId,
            _currentUser.Role,
            true);
    }

    private static AuthenticationState BuildAuthenticationState(AuthenticatedUserDto user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, UserRoles.Normalize(user.Role))
        };

        if (user.StudentId.HasValue)
        {
            claims.Add(new Claim("StudentId", user.StudentId.Value.ToString()));
        }

        if (!string.IsNullOrWhiteSpace(user.StudentNumber))
        {
            claims.Add(new Claim("StudentNumber", user.StudentNumber));
        }

        var identity = new ClaimsIdentity(claims, authenticationType: "CrmAuth");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }
}
