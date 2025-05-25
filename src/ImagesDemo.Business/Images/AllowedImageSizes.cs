namespace ImagesDemo.Business.Images;

public class AllowedImageSizes
{
    public static (int Width, int Height) ProfilePictureSize { get; } = (500, 500);
    public static (int Width, int Height) MaxUserImageSize { get; } = (2560, 2560);

    private static HashSet<(int, int)> ImageSizes { get; }

    static AllowedImageSizes()
    {
        ImageSizes = new HashSet<(int, int)>
            {
                ProfilePictureSize, //this is currently (500, 500) but in case this changes we probably want to keep (500, 500) separately,
                MaxUserImageSize,
                (500, 500),
                (100, 100),
                (500, 250)
                //Add allowed sizes here as needed
            };
    }

    public static bool IsImageSizeAllowed(int width, int height)
    {
        return ImageSizes.Contains((width, height));
    }
}
