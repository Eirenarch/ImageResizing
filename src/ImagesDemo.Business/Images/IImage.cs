namespace ImagesDemo.Business.Images;

public interface IImage
{
    string ContentType { get; set; }
    int Width { get; set; }
    int Height { get; set; }
    string BlobFileName { get; set; }
    DateTime Timestamp { get; set; }
}