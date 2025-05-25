namespace ImagesDemo.Business.Images;

using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

public class FileSystemBlobStorageClient : IImageBlobStorageClient
{
    private string BasePath { get; }

    public FileSystemBlobStorageClient(IConfiguration configuration)
    {
        BasePath = configuration["FileSystemBlobStorage:BasePath"]
            ?? throw new InvalidOperationException("Missing configuration for FileSystemBlobStorage:BasePath");
    }

    public async Task DownloadImageAsync(string fileName, Stream outputStream)
    {
        using FileStream fileStream = File.OpenRead(Path.Combine(BasePath, fileName));
        await fileStream.CopyToAsync(outputStream);
    }

    public async Task UploadImageAsync(string fileName, Stream sourceStream, string contentType)
    {
        using FileStream fileStream = File.Create(Path.Combine(BasePath, fileName));
        await sourceStream.CopyToAsync(fileStream);
    }
}