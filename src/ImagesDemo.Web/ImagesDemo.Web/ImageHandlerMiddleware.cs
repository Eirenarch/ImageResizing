namespace ImagesDemo.Web;


using ImagesDemo.Business.Images;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Net.Http.Headers;

public class ImageHandlerMiddleware
{
    private const double LastModifiedEpsilonSeconds = 1;

    public ImageHandlerMiddleware(RequestDelegate next)
    {
    }

    public async Task InvokeAsync(HttpContext context)
    {
        //This handler does not contain any security checks. All images are accessible

        if (context.Request.Method != "GET")
        {
            context.Response.StatusCode = 405;
            return;
        }

        if (!Guid.TryParse(context.Request.Query["imageId"], out Guid imageId))
        {
            context.Response.StatusCode = 400;
            return;
        }

        int? width = null;
        int? height = null;

        if (context.Request.Query["width"].Count > 0 || context.Request.Query["height"].Count > 0)
        {
            if (!Int32.TryParse(context.Request.Query["width"], out int tempWidth))
            {
                context.Response.StatusCode = 400;
                return;
            }

            if (!Int32.TryParse(context.Request.Query["height"], out int tempHeight))
            {
                context.Response.StatusCode = 400;
                return;
            }

            width = tempWidth;
            height = tempHeight;
        }

        ImageResizeMode imageResizeMode = ImageResizeMode.Resize;

        string? imageResizeModeQuery = context.Request.Query["mode"].ToString()?.ToLower();

        if (imageResizeModeQuery == "crop")
        {
            imageResizeMode = ImageResizeMode.Crop;
        }
        else if(imageResizeModeQuery == "pad")
        {
            imageResizeMode = ImageResizeMode.Pad;
        }

            IImageService imageService = (IImageService)context.RequestServices.GetService(typeof(IImageService))!;

        IImage? image;

        try
        {
            if (width != null)
            {
                image = await imageService.GetImageMetadataAsync(imageId, width.Value, height!.Value, imageResizeMode);
            }
            else
            {
                image = await imageService.GetOriginalImageMetadataAsync(imageId);
            }
        }
        catch (ImageSizeNotAllowedException)
        {
            context.Response.StatusCode = 400;
            return;
        }

        if (image == null)
        {
            context.Response.StatusCode = 404;
            return;
        }

        // Currently if the header is present the second condition should always be true because we're not modifying images
        if (DateTime.TryParse(context.Request.Headers.IfModifiedSince, out DateTime lastModified)
         && lastModified.AddSeconds(LastModifiedEpsilonSeconds) >= image.Timestamp)
        {
            context.Response.StatusCode = 304;
            return;
        }

        ResponseHeaders responseHeaders = context.Response.GetTypedHeaders();
        responseHeaders.CacheControl = new CacheControlHeaderValue
        {
            Private = true,
            MaxAge = TimeSpan.FromDays(365)
        };
        responseHeaders.LastModified = image.Timestamp;
        responseHeaders.Expires = DateTime.UtcNow.AddDays(365);

        context.Response.ContentType = image.ContentType;
        await imageService.DownloadImageBlobAsync(image.BlobFileName, context.Response.Body);
        await context.Response.Body.FlushAsync();
    }

}

public static class ImageHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseImageHandlerMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ImageHandlerMiddleware>();
    }
}
