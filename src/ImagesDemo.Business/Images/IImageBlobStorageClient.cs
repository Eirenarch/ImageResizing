namespace ImagesDemo.Business.Images;

public interface IImageBlobStorageClient
{
    Task UploadImageAsync(string fileName, Stream sourceStream, string contentType);
    Task DownloadImageAsync(string fileName, Stream outputStream);
}