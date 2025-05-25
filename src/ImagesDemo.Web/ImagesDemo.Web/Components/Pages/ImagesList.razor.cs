namespace ImagesDemo.Web.Components.Pages;

using ImagesDemo.Business;
using ImagesDemo.Business.Images;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

public partial class ImagesList : ComponentBase
{
    [Inject]
    private ImagesDemoContext DbContext { get; set; } = null!;

    private List<ImageInfo> ImageInfos { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        ImageInfos = await DbContext.ImageInfos
            .OrderByDescending(i => i.Timestamp)
            .ToListAsync();
    }
}
