namespace ImagesDemo.Business.Images;

using ImageDull.ImageSharp;
using ImageDull.ImageSharp.Formats;
using ImageDull.ImageSharp.Processing;

public class ImageService : IImageService
{
    private IImageBlobStorageClient BlobStorageClient { get; }
    private ImagesDemoContext Context { get; }

    public ImageService(IImageBlobStorageClient blobStorageClient, ImagesDemoContext imagesDemoContext)
    {
        BlobStorageClient = blobStorageClient;
        Context = imagesDemoContext;
    }

    public async Task<IImage?> GetOriginalImageMetadataAsync(Guid imageInfoId)
    {
        ImageInfo? imageInfo = await Context.ImageInfos.SingleOrDefaultAsync(ii => ii.ImageInfoId == imageInfoId);
        return imageInfo;
    }

    public async Task<IImage?> GetImageMetadataAsync(Guid imageInfoId, int width, int height, ImageResizeMode imageResizeMode)
    {
        ImageInfo? imageInfo = await Context.ImageInfos
            .Include(ii => ii.ImageVersions.Where(iv => iv.Width == width && iv.Height == height && iv.ImageResizeMode == imageResizeMode))
            .SingleOrDefaultAsync(ii => ii.ImageInfoId == imageInfoId);

        if (imageInfo is null)
        {
            return null;
        }

        if ((imageInfo.Width == width && imageInfo.Height == height)
         || (imageResizeMode == ImageResizeMode.Resize && imageInfo.Width <= width && imageInfo.Height <= height))
        {
            return imageInfo;
        }

        ThrowIfWidthOrHeightInvalid(width, height); //only check image sizes if a resize is needed

        //I am not using SingleOrDefault here because the CreateImageVersion has concurrency issues and we may end up with identical ImageVersions
        ImageVersion? imageVersion = imageInfo.ImageVersions
            .FirstOrDefault(iv => iv.Width == width && iv.Height == height && iv.ImageResizeMode == imageResizeMode);

        if (imageVersion is not null)
        {
            return imageVersion;
        }

        return await CreateImageVersionAsync(imageInfo, width, height, imageResizeMode);
    }

    private async Task<ImageVersion> CreateImageVersionAsync(ImageInfo imageInfo, int width, int height, ImageResizeMode imageResizeMode)
    {
        //there are concurrency issues here. It is possible that two requests call this method at the same time then we will end up with two identical ImageVersions
        //currently this is not a problem because the versions will be identical. The only problem is a small amount of useless images generated.
        using MemoryStream sourceStream = new();
        await BlobStorageClient.DownloadImageAsync(imageInfo.BlobFileName, sourceStream);
        sourceStream.Seek(0, SeekOrigin.Begin);

        string fileName = await SaveImageInternalAsync(sourceStream, width, height, null, imageResizeMode);

        ImageVersion newImageVersion = new()
        {
            BlobFileName = fileName,
            ContentType = fileName.EndsWith(".png") ? "image/png" : "image/jpeg",
            Height = height,
            Width = width,
            ImageInfo = imageInfo,
            ImageInfoId = imageInfo.ImageInfoId,
            Timestamp = DateTime.UtcNow,
            ImageResizeMode = imageResizeMode
        };

        imageInfo.ImageVersions.Add(newImageVersion);
        await Context.SaveChangesAsync();

        return newImageVersion;
    }

    public async Task DownloadImageBlobAsync(string fileName, Stream outputStream)
    {
        ArgumentNullException.ThrowIfNull(outputStream);

        await BlobStorageClient.DownloadImageAsync(fileName, outputStream);
    }

    public Task<ImageInfo> SaveJpgImageAsync(Stream sourceStream, int width, int height, ImageResizeMode imageResizeMode, string currentUserId)
    {
        return SaveResizedImageAsync(sourceStream, width, height, imageResizeMode, currentUserId, ".jpg", "image/jpeg",
            (image, stream) => image.SaveAsJpegAsync(stream));
    }

    public Task<ImageInfo> SavePngImageAsync(Stream sourceStream, int width, int height, ImageResizeMode imageResizeMode, string currentUserId)
    {
        return SaveResizedImageAsync(sourceStream, width, height, imageResizeMode, currentUserId, ".png", "image/png",
            (image, stream) => image.SaveAsPngAsync(stream));
    }

    private async Task<ImageInfo> SaveResizedImageAsync(
        Stream sourceStream,
        int width,
        int height,
        ImageResizeMode imageResizeMode,
        string currentUserId,
        string fileExtension,
        string contentType,
        Func<Image, Stream, Task> encodeAsync)
    {
        ArgumentNullException.ThrowIfNull(sourceStream);

        using MemoryStream memoryStream = new();
        ResizeOptions resizeOptions = new()
        {
            Mode = GetResizeMode(imageResizeMode),
            Size = new Size(width, height)
        };

        int finalWidth = 0;
        int finalHeight = 0;

        string fileName = Guid.NewGuid().ToString();

        Image image = await Image.LoadAsync(sourceStream);

        using (image)
        {
            image.Mutate(x => x.AutoOrient()); //Android is a cancer. Images come with some rotation
            image.Metadata.ExifProfile = null;

            if (image.Width != width || image.Height != height)
            {
                image.Mutate(x => x.Resize(resizeOptions));
            }

            fileName += fileExtension;
            await encodeAsync(image, memoryStream);

            finalWidth = image.Width;
            finalHeight = image.Height;
        }

        memoryStream.Seek(0, SeekOrigin.Begin);

        await BlobStorageClient.UploadImageAsync(fileName, memoryStream, contentType);

        ImageInfo imageInfo = new()
        {
            BlobFileName = fileName,
            ContentType = contentType,
            Width = finalWidth,
            Height = finalHeight,
            Timestamp = DateTime.UtcNow,
            UploaderId = currentUserId
        };

        Context.ImageInfos.Add(imageInfo);
        await Context.SaveChangesAsync();

        return imageInfo;
    }

    public async Task<ImageInfo> SaveImageAsync(Stream sourceStream, string currentUserId)
    {
        ArgumentNullException.ThrowIfNull(sourceStream);

        using MemoryStream memoryStream = new();
        ResizeOptions resizeOptions = new()
        {
            Mode = ResizeMode.Max,
            Size = new Size(AllowedImageSizes.MaxUserImageSize.Width, AllowedImageSizes.MaxUserImageSize.Height)
        };

        int finalWidth = 0;
        int finalHeight = 0;

        string fileName = Guid.NewGuid().ToString();

        (Image image, IImageFormat format) = await Image.LoadWithFormatAsync(sourceStream);

        using (image)
        {
            image.Mutate(x => x.AutoOrient()); //Android is a cancer. Images come with some rotation
            image.Metadata.ExifProfile = null;

            if (image.Width > AllowedImageSizes.MaxUserImageSize.Width || image.Height > AllowedImageSizes.MaxUserImageSize.Height)
            {
                image.Mutate(x => x.Resize(resizeOptions));
            }

            if (format.DefaultMimeType == "image/png")
            {
                fileName += ".png";
                await image.SaveAsPngAsync(memoryStream);
            }
            else
            {
                fileName += ".jpg";
                await image.SaveAsJpegAsync(memoryStream);
            }

            finalWidth = image.Width;
            finalHeight = image.Height;
        }

        memoryStream.Seek(0, SeekOrigin.Begin);

        await BlobStorageClient.UploadImageAsync(fileName, memoryStream, format.DefaultMimeType);

        ImageInfo imageInfo = new()
        {
            BlobFileName = fileName,
            ContentType = format.DefaultMimeType,
            Width = finalWidth,
            Height = finalHeight,
            Timestamp = DateTime.UtcNow,
            UploaderId = currentUserId
        };

        Context.ImageInfos.Add(imageInfo);
        await Context.SaveChangesAsync();

        return imageInfo;
    }

    public Task<string> SaveImageAsync(Stream sourceStream, int width, int height, ImageResizeMode imageResizeMode)
    {
        return SaveImageInternalAsync(sourceStream, width, height, null, imageResizeMode);
    }

    public Task<string> SaveImageAsync(Stream sourceStream, int width, int height)
    {
        return SaveImageInternalAsync(sourceStream, width, height, null, ImageResizeMode.Pad);
    }

    public Task<string> SaveImageAsync(Stream sourceStream, int width, int height, int[] cropValues)
    {
        return SaveImageInternalAsync(sourceStream, width, height, cropValues, ImageResizeMode.Pad);
    }

    private async Task<string> SaveImageInternalAsync(Stream sourceStream, int width, int height, int[]? cropValues, ImageResizeMode imageResizeMode)
    {
        ArgumentNullException.ThrowIfNull(sourceStream);
        ThrowIfWidthOrHeightInvalid(width, height);

        using MemoryStream memoryStream = new();
        string fileName = Guid.NewGuid().ToString();
        ResizeMode resizeMode = GetResizeMode(imageResizeMode);

        ResizeOptions resizeOptions = new()
        {
            Mode = resizeMode,
            Size = new Size(width, height)
        };

        (Image image, IImageFormat format) = await Image.LoadWithFormatAsync(sourceStream);

        using (image)
        {
            if (image.Width != width || image.Height != height)
            {
                image.Mutate(x =>
                {
                    if (cropValues != null)
                    {
                        x = x.Crop(new Rectangle(x: cropValues[0], y: cropValues[1], width: cropValues[2], height: cropValues[3]));
                        throw new NotImplementedException("This has never been tested");
                    }

                    if (format.DefaultMimeType == "image/png")
                    {
                        x.Resize(resizeOptions).BackgroundColor(Color.Transparent);
                    }
                    else
                    {
                        x.Resize(resizeOptions).BackgroundColor(Color.White);
                    }
                });

                fileName += $"W{width}H{height}M{imageResizeMode}";
            }

            if (format.DefaultMimeType == "image/png")
            {
                fileName += ".png";
                await image.SaveAsPngAsync(memoryStream);
            }
            else
            {
                fileName += ".jpg";
                await image.SaveAsJpegAsync(memoryStream);
            }
        }

        memoryStream.Seek(0, SeekOrigin.Begin);

        await BlobStorageClient.UploadImageAsync(fileName, memoryStream, format.DefaultMimeType);

        return fileName;
    }

    private static ResizeMode GetResizeMode(ImageResizeMode imageResizeMode)
    {
        return imageResizeMode switch
        {
            ImageResizeMode.Pad => ResizeMode.Pad,
            ImageResizeMode.Crop => ResizeMode.Crop,
            ImageResizeMode.Resize => ResizeMode.Max,
            _ => throw new Exception($"Unexpected ImageResizeMode:{imageResizeMode}")
        };
    }

    private static void ThrowIfWidthOrHeightInvalid(int width, int height)
    {
        if (!AllowedImageSizes.IsImageSizeAllowed(width, height))
        {
            throw new ImageSizeNotAllowedException($"Width:{width}, Height:{height}");
        }
    }
}
