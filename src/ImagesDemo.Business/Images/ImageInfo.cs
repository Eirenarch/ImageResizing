namespace ImagesDemo.Business.Images;

[Table("ImageInfos")]
public class ImageInfo : IImage
{
    public ImageInfo()
    {
        ImageVersions = new HashSet<ImageVersion>();
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid ImageInfoId { get; set; }

    [MaxLength(64)]
    public string ContentType { get; set; } = null!;

    public int Width { get; set; }
    public int Height { get; set; }

    public ICollection<ImageVersion> ImageVersions { get; set; }

    [MaxLength(64)]
    public string BlobFileName { get; set; } = null!;

    [ForeignKey(nameof(Uploader))]
    public string UploaderId { get; set; } = null!;
    public User Uploader { get; set; } = null!;

    public DateTime Timestamp { get; set; }
}