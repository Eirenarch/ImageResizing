namespace ImagesDemo.Business.Images;

public class ImageSizeNotAllowedException : Exception
{
    public ImageSizeNotAllowedException()
    {
    }

    public ImageSizeNotAllowedException(string message) : base(message)
    {
    }
}