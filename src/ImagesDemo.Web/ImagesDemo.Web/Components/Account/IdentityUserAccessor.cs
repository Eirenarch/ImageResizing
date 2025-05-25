namespace ImagesDemo.Web.Components.Account;

using ImagesDemo.Business.Account;
using Microsoft.AspNetCore.Identity;

internal sealed class IdentityUserAccessor(UserManager<User> userManager, IdentityRedirectManager redirectManager)
{
    public async Task<User> GetRequiredUserAsync(HttpContext context)
    {
        User? user = await userManager.GetUserAsync(context.User);

        if (user is null)
        {
            redirectManager.RedirectTo("Account/InvalidUser");
        }

        return user;
    }
}
