namespace ImagesDemo.Web.Components.Account.Pages;

using ImagesDemo.Business.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

public partial class Login
{
    private string? ErrorMessage { get; set; }

    [CascadingParameter]
    private HttpContext HttpContext { get; set; } = null!;

    [SupplyParameterFromForm]
    private InputModel Input { get; set; } = new();

    [SupplyParameterFromQuery]
    private string? ReturnUrl { get; set; }

    [Inject]
    protected SignInManager<User> SignInManager { get; set; } = null!;

    [Inject]
    protected UserManager<User> UserManager { get; set; } = null!;

    [Inject]
    protected NavigationManager NavigationManager { get; set; } = null!;

    [Inject]
    protected IdentityRedirectManager RedirectManager { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        if (HttpMethods.IsGet(HttpContext.Request.Method))
        {
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        }
    }

    public async Task LoginUserAsync()
    {
        User? user = await UserManager.FindByEmailAsync(Input.Email);

        if (user is null)
        {
            user = new User
            {
                UserName = Input.Email,
                Email = Input.Email,
                EmailConfirmed = true,
                LockoutEnabled = false
            };

            IdentityResult result = await UserManager.CreateAsync(user, Input.Password);

            if (!result.Succeeded)
            {
                ErrorMessage = $"Error creating user: {String.Join(", ", result.Errors.Select(e => e.Description))}";
                return;
            }
        }

        SignInResult signInResult = await SignInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);

        if (signInResult.Succeeded)
        {
            RedirectManager.RedirectTo(ReturnUrl);
        }
        else
        {
            ErrorMessage = "Error: Invalid login attempt.";
        }
    }

    private sealed class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}