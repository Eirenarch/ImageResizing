namespace ImagesDemo.Business.Images;

public class ImageVersion : IImage
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid ImageVersionId { get; set; }

    [ForeignKey(nameof(ImageInfo))]
    public Guid ImageInfoId { get; set; }
    public ImageInfo ImageInfo { get; set; } = null!;

    [MaxLength(64)]
    public string ContentType { get; set; } = null!;

    public int Width { get; set; }
    public int Height { get; set; }
    public ImageResizeMode ImageResizeMode { get; set; }

    [MaxLength(64)]
    public string BlobFileName { get; set; } = null!;

    public DateTime Timestamp { get; set; }
}