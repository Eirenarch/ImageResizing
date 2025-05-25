namespace ImagesDemo.Business.Images;

public interface IImageService
{
    /// <summary>
    /// Gets the image metadata
    /// </summary>
    /// <param name="imageInfoId">The id of the ImageInfo in the database</param>
    /// <returns>Image metadata or null if the original image does not exist</returns>
    Task<IImage?> GetOriginalImageMetadataAsync(Guid imageInfoId);

    /// <summary>
    /// Gets the image metadata
    /// </summary>
    /// <param name="imageInfoId">The id of the ImageInfo in the database</param>
    /// <param name="width">The image width</param>
    /// <param name="height">The image height</param>
    /// <param name="imageResizeMode">The image resize mode. Pad - maintain proportions, pad to fit the desired size, Crop - crop part of the image with the desired proportions and resize if needed, Resize - resize to fit a bounding rectangle</param>
    /// <returns>Image metadata or null if the original image does not exist</returns>
    /// <remarks>If the image does not exist with the specified width and height a new ImageVersion is created from the original and a resized blob is saved to the storage. This method will throw ImageSizeNotAllowedException if the requested image size is not in the list of allowed sizes.</remarks>
    Task<IImage?> GetImageMetadataAsync(Guid imageInfoId, int width, int height, ImageResizeMode imageResizeMode);

    /// <summary>
    /// Downloads an image from blob storage into the specified output stream.
    /// </summary>
    /// <param name="fileName">The name of the file in the blob storage</param>
    /// <param name="outputStream">The output stream to which the image data should be written</param>
    Task DownloadImageBlobAsync(string fileName, Stream outputStream);

    /// <summary>
    /// Saves an image to the blob storage.
    /// </summary>
    /// <param name="sourceStream">The stream with the source image data</param>
    /// <param name="currentUserId">The id of the user uploading the file</param>
    /// <returns>The ImageInfo for the newly uploaded image</returns>
    Task<ImageInfo> SaveImageAsync(Stream sourceStream, string currentUserId);

    /// <summary>
    /// Saves an image to the blob storage with the specified file name and resizes to the proper dimensions. Smaller images will be upscaled. If the aspect ratio does not match images will be padded.
    /// </summary>
    /// <param name="sourceStream">The stream with the source image data</param>
    /// <param name="width">The resize width</param>
    /// <param name="height">The resize height</param>
    /// <returns>The name of the file in the blob storage</returns>
    Task<string> SaveImageAsync(Stream sourceStream, int width, int height);

    /// <summary>
    /// Saves an image to the blob storage with the specified file name and resizes to the proper dimensions. Smaller images will be upscaled.
    /// </summary>
    /// <param name="sourceStream">The stream with the source image data</param>
    /// <param name="width">The resize width</param>
    /// <param name="height">The resize height</param>
    /// <param name="imageResizeMode">Indicates what the resizer should do if the proportions do not match</param>
    /// <returns>The name of the file in the blob storage</returns>
    Task<string> SaveImageAsync(Stream sourceStream, int width, int height, ImageResizeMode imageResizeMode);

    /// <summary>
    /// Saves an image to the blob storage with the specified file name and resizes to the proper dimensions. Smaller images will be upscaled.
    /// </summary>
    /// <param name="sourceStream">The stream with the source image data</param>
    /// <param name="width">The resize width</param>
    /// <param name="height">The resize height</param>
    /// <param name="imageResizeMode">Indicates what the resizer should do if the proportions do not match</param>
    /// <param name="currentUserId">The id of the user uploading the file</param>
    /// <returns>The ImageInfo for the newly uploaded image</returns>
    Task<ImageInfo> SaveImageAsync(Stream sourceStream, int width, int height, ImageResizeMode imageResizeMode, string currentUserId);

    /// <summary>
    /// Saves an image to the blob storage with the specified file name and resizes to the proper dimensions. Smaller images will be upscaled. If the aspect ratio does not match images will be padded.
    /// </summary>
    /// <param name="sourceStream">The stream with the source image data</param>
    /// <param name="width">The resize width</param>
    /// <param name="height">The resize height</param>
    /// <param name="cropValues">The crop values (x, y, width, height)</param>
    /// <returns>The name of the file in the blob storage</returns>
    Task<string> SaveImageAsync(Stream sourceStream, int width, int height, int[] cropValues);
}
