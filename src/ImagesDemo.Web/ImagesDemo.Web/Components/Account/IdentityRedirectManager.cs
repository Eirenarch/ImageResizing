namespace ImagesDemo.Web.Components.Account;

using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;

public sealed class IdentityRedirectManager
{
    private NavigationManager NavigationManager { get; }

    public IdentityRedirectManager(NavigationManager navigationManager)
    {
        NavigationManager = navigationManager;
    }

    [DoesNotReturn]
    public void RedirectTo(string? uri)
    {
        uri ??= "";

        // Prevent open redirects.
        if (!Uri.IsWellFormedUriString(uri, UriKind.Relative))
        {
            uri = NavigationManager.ToBaseRelativePath(uri);
        }

        // During static rendering, NavigateTo throws a NavigationException which is handled by the framework as a redirect.
        // So as long as this is called from a statically rendered Identity component, the InvalidOperationException is never thrown.
        NavigationManager.NavigateTo(uri);
        throw new InvalidOperationException($"{nameof(IdentityRedirectManager)} can only be used during static rendering.");
    }

    [DoesNotReturn]
    public void RedirectTo(string uri, Dictionary<string, object?> queryParameters)
    {
        string uriWithoutQuery = NavigationManager.ToAbsoluteUri(uri).GetLeftPart(UriPartial.Path);
        string newUri = NavigationManager.GetUriWithQueryParameters(uriWithoutQuery, queryParameters);
        RedirectTo(newUri);
    }

    private string CurrentPath
    {
        get
        {
            return NavigationManager.ToAbsoluteUri(NavigationManager.Uri).GetLeftPart(UriPartial.Path);
        }
    }

    [DoesNotReturn]
    public void RedirectToCurrentPage()
    {
        RedirectTo(CurrentPath);
    }
}
