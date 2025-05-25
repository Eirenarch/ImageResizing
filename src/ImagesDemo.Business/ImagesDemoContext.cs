namespace ImagesDemo.Business;

using ImagesDemo.Business.Images;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

public class ImagesDemoContext : IdentityDbContext<User>
{
    public ImagesDemoContext(DbContextOptions<ImagesDemoContext> options) : base(options)
    {
    }

    public DbSet<ImageInfo> ImageInfos { get; set; } = null!;
    public DbSet<ImageVersion> ImageVersions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        //all dates are UTC
        ValueConverter<DateTime, DateTime> dateTimeConverter = new(
            v => v.ToUniversalTime(),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        ValueConverter<DateTime?, DateTime?> nullableDateTimeConverter = new(
            v => v.HasValue ? v.Value.ToUniversalTime() : v,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

        foreach (IMutableEntityType entityType in builder.Model.GetEntityTypes())
        {
            foreach (IMutableProperty property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                {
                    property.SetValueConverter(dateTimeConverter);
                }
                else if (property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(nullableDateTimeConverter);
                }
            }
        }

        base.OnModelCreating(builder);
    }
}