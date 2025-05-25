namespace ImagesDemo.Web.Components.Pages;

using ImagesDemo.Business.Account;
using ImagesDemo.Business.Images;
using ImagesDemo.Web.Components.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

[Authorize]
public partial class ImageUpload : ComponentBase
{
    protected string? UploadMessage { get; set; }

    [CascadingParameter]
    private HttpContext HttpContext { get; set; } = null!;

    [Inject]
    private IdentityUserAccessor IdentityUserAccessor { get; set; } = null!;

    [Inject]
    private IImageService ImageService { get; set; } = null!;

    [SupplyParameterFromForm]
    private IFormFile? File { get; set; }

    protected async Task SubmitAsync()
    {
        if (File is null)
        {
            UploadMessage = "No file selected.";
            return;
        }

        User user = await IdentityUserAccessor.GetRequiredUserAsync(HttpContext);
        using Stream stream = File.OpenReadStream();
        ImageInfo imageInfo = await ImageService.SaveImageAsync(stream, user.Id);

        UploadMessage = $"Uploaded {File.Name} ({imageInfo.ImageInfoId})(Size: {File.Length / 1024} KB)";
    }
}
